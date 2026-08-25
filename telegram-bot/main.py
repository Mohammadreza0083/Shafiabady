import asyncio
import contextlib
import html
import logging
import os
from datetime import datetime
from typing import Any, Dict, Optional

import httpx
from dotenv import load_dotenv
from fastapi import FastAPI, HTTPException, status
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel, ConfigDict, Field
from telegram import Bot
from telegram.constants import ParseMode
from telegram.error import TelegramError

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s [%(levelname)s] %(name)s: %(message)s"
)
logger = logging.getLogger("telegram_webhook_service")

# Load environment variables
load_dotenv()

TELEGRAM_BOT_TOKEN = os.getenv("TELEGRAM_BOT_TOKEN")
TELEGRAM_CHAT_ID = os.getenv("TELEGRAM_CHAT_ID", "269309616")
TELEGRAM_ADMIN_CHAT_ID = os.getenv("TELEGRAM_ADMIN_CHAT_ID", TELEGRAM_CHAT_ID or "269309616")
ALLOWED_ORIGINS_STR = os.getenv("ALLOWED_ORIGINS", "http://localhost:4500,http://127.0.0.1:4500,http://localhost:3000")
ALLOWED_ORIGINS = [origin.strip() for origin in ALLOWED_ORIGINS_STR.split(",") if origin.strip()]

# Monitoring endpoints
BACKEND_HEALTH_URL = os.getenv("BACKEND_HEALTH_URL", "http://backend:5000/health")
FRONTEND_HEALTH_URL = os.getenv("FRONTEND_HEALTH_URL", "http://frontend:4500/")
HEALTH_CHECK_INTERVAL_SECONDS = int(os.getenv("HEALTH_CHECK_INTERVAL_SECONDS", "120"))

if not TELEGRAM_BOT_TOKEN or not TELEGRAM_CHAT_ID:
    logger.warning("TELEGRAM_BOT_TOKEN or TELEGRAM_CHAT_ID is not configured properly in environment")

# Initialize Telegram Bot instance
bot: Optional[Bot] = None
if TELEGRAM_BOT_TOKEN:
    bot = Bot(token=TELEGRAM_BOT_TOKEN)

# In-memory health state tracking for monitored services
service_states: Dict[str, Dict[str, Any]] = {
    "Backend .NET API": {
        "url": BACKEND_HEALTH_URL,
        "is_healthy": True,
        "failure_count": 0,
        "last_checked": None,
        "last_error": None,
    },
    "Frontend Astro": {
        "url": FRONTEND_HEALTH_URL,
        "is_healthy": True,
        "failure_count": 0,
        "last_checked": None,
        "last_error": None,
    }
}


async def send_emergency_telegram_alert(service_name: str, url: str, error_msg: str, failure_count: int) -> None:
    """Dispatches high-priority emergency failure alerts to Telegram admin chat."""
    if not bot or not TELEGRAM_ADMIN_CHAT_ID:
        logger.error("Cannot send emergency alert: Telegram bot or admin chat ID is missing")
        return

    now_str = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
    safe_service = html.escape(service_name)
    safe_url = html.escape(url)
    safe_error = html.escape(error_msg)

    alert_text = (
        "🚨 <b>هشدار اضطراری زیرساخت: قطع یا اختلال در سرویس</b>\n"
        "━━━━━━━━━━━━━━━━━━\n"
        f"🌐 <b>سرویس آسیب‌دیده:</b> <code>{safe_service}</code>\n"
        f"🔗 <b>آدرس بررسی:</b> <code>{safe_url}</code>\n"
        f"⚠️ <b>خطای دریافتی:</b> <code>{safe_error}</code>\n"
        f"🔢 <b>تعداد خطاهای متوالی:</b> {failure_count}\n"
        "━━━━━━━━━━━━━━━━━━\n"
        f"⏱ <i>زمان ثبت رخداد: {now_str}</i>\n"
        "<i>لطفاً وضعیت کانتینرها و لاگ‌های سرور را بررسی فرمایید.</i>"
    )

    try:
        await bot.send_message(
            chat_id=TELEGRAM_ADMIN_CHAT_ID,
            text=alert_text,
            parse_mode=ParseMode.HTML,
            disable_web_page_preview=True
        )
        logger.info("Dispatched emergency alert for %s to chat %s", service_name, TELEGRAM_ADMIN_CHAT_ID)
    except Exception as exc:
        logger.error("Failed to send emergency Telegram alert: %s", str(exc), exc_info=True)


async def send_recovery_telegram_alert(service_name: str, url: str) -> None:
    """Dispatches recovery notification to Telegram admin when service returns to healthy state."""
    if not bot or not TELEGRAM_ADMIN_CHAT_ID:
        return

    now_str = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
    safe_service = html.escape(service_name)
    safe_url = html.escape(url)

    recovery_text = (
        "✅ <b>اطلاعیه بازیابی سرویس: وضعیت پایدار شد</b>\n"
        "━━━━━━━━━━━━━━━━━━\n"
        f"🌐 <b>سرویس:</b> <code>{safe_service}</code>\n"
        f"🔗 <b>آدرس بررسی:</b> <code>{safe_url}</code>\n"
        "📊 <b>وضعیت کنونی:</b> پایدار (200 OK)\n"
        "━━━━━━━━━━━━━━━━━━\n"
        f"⏱ <i>زمان بازیابی: {now_str}</i>"
    )

    try:
        await bot.send_message(
            chat_id=TELEGRAM_ADMIN_CHAT_ID,
            text=recovery_text,
            parse_mode=ParseMode.HTML,
            disable_web_page_preview=True
        )
        logger.info("Dispatched recovery notification for %s to chat %s", service_name, TELEGRAM_ADMIN_CHAT_ID)
    except Exception as exc:
        logger.error("Failed to send recovery Telegram alert: %s", str(exc), exc_info=True)


async def ping_service(client: httpx.AsyncClient, service_name: str, config: Dict[str, Any]) -> None:
    """Pings an individual service endpoint and manages failure state transitions."""
    url = config["url"]
    config["last_checked"] = datetime.now().isoformat()

    try:
        response = await client.get(url, timeout=10.0)
        # Treat 2xx and 3xx as healthy
        if 200 <= response.status_code < 400:
            if not config["is_healthy"]:
                logger.info("Service '%s' has RECOVERED (Status %d)", service_name, response.status_code)
                config["is_healthy"] = True
                config["failure_count"] = 0
                config["last_error"] = None
                await send_recovery_telegram_alert(service_name, url)
            else:
                config["failure_count"] = 0
                config["last_error"] = None
        else:
            # 4xx or 5xx error
            err_msg = f"HTTP Status {response.status_code}"
            config["failure_count"] += 1
            config["last_error"] = err_msg
            logger.warning("Service '%s' returned unhealthy status: %s", service_name, err_msg)
            
            # Alert on first failure or periodic reminders every 5 failures
            if config["is_healthy"] or config["failure_count"] % 5 == 1:
                config["is_healthy"] = False
                await send_emergency_telegram_alert(service_name, url, err_msg, config["failure_count"])

    except (httpx.ConnectError, httpx.TimeoutException, httpx.NetworkError, Exception) as exc:
        err_msg = f"{type(exc).__name__}: {str(exc) or 'Connection failed / Timeout'}"
        config["failure_count"] += 1
        config["last_error"] = err_msg
        logger.error("Health check failed for '%s' (%s): %s", service_name, url, err_msg)

        # Alert on first failure or periodic reminders
        if config["is_healthy"] or config["failure_count"] % 5 == 1:
            config["is_healthy"] = False
            await send_emergency_telegram_alert(service_name, url, err_msg, config["failure_count"])


async def health_monitor_daemon() -> None:
    """Asynchronous background daemon running every HEALTH_CHECK_INTERVAL_SECONDS."""
    logger.info("Starting background health monitor daemon (Interval: %ds)...", HEALTH_CHECK_INTERVAL_SECONDS)
    # Initial grace period on container startup
    await asyncio.sleep(15)

    async with httpx.AsyncClient(verify=False, follow_redirects=True) as client:
        while True:
            try:
                # Update URLs in case environment was modified
                service_states["Backend .NET API"]["url"] = os.getenv("BACKEND_HEALTH_URL", BACKEND_HEALTH_URL)
                service_states["Frontend Astro"]["url"] = os.getenv("FRONTEND_HEALTH_URL", FRONTEND_HEALTH_URL)

                for service_name, config in list(service_states.items()):
                    await ping_service(client, service_name, config)

            except asyncio.CancelledError:
                logger.info("Health monitor daemon received cancellation request.")
                break
            except Exception as e:
                logger.error("Unexpected error inside health monitor daemon: %s", str(e), exc_info=True)

            await asyncio.sleep(HEALTH_CHECK_INTERVAL_SECONDS)


@contextlib.asynccontextmanager
async def lifespan(fastapi_app: FastAPI):
    """FastAPI application lifespan context managing background monitor daemon."""
    logger.info("Starting up Telegram Bot & Alert Microservice...")
    monitor_task = asyncio.create_task(health_monitor_daemon())
    yield
    logger.info("Shutting down Telegram Bot microservice...")
    monitor_task.cancel()
    try:
        await monitor_task
    except asyncio.CancelledError:
        pass


# Initialize FastAPI App with Lifespan
app = FastAPI(
    title="ShafieiAbadi Telegram Bot & Health Daemon",
    description="Microservice for Contact Webhook dispatch and Proactive Infrastructure Health Alerting",
    version="2.0.0",
    lifespan=lifespan
)

# Configure CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=ALLOWED_ORIGINS if ALLOWED_ORIGINS else ["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


class ContactRequest(BaseModel):
    name: str = Field(..., min_length=1, description="Sender's full name", validation_alias="fullName")
    email: str = Field(..., min_length=3, description="Sender's email address")
    message: str = Field(..., min_length=1, description="Message content")
    phone: Optional[str] = Field(default=None, description="Sender's contact phone number", validation_alias="phoneNumber")
    telegram_id: Optional[str] = Field(default=None, description="Sender's Telegram handle", validation_alias="telegramId")

    model_config = ConfigDict(
        populate_by_name=True,
        extra="ignore"
    )


def format_telegram_message(payload: ContactRequest) -> str:
    """Safely format contact submission into Telegram HTML."""
    safe_name = html.escape(payload.name.strip())
    safe_email = html.escape(payload.email.strip())
    safe_message = html.escape(payload.message.strip())
    safe_phone = html.escape(payload.phone.strip()) if payload.phone and payload.phone.strip() else "ثبت نشده"

    raw_telegram = payload.telegram_id.strip() if payload.telegram_id else ""
    if raw_telegram:
        clean_handle = raw_telegram.lstrip("@")
        safe_telegram = f'<a href="https://t.me/{html.escape(clean_handle)}">@{html.escape(clean_handle)}</a>'
    else:
        safe_telegram = "ثبت نشده"

    now_str = datetime.now().strftime("%Y-%m-%d %H:%M:%S")

    return (
        "📬 <b>پیام جدید از فرم تماس وب‌سایت</b>\n"
        "━━━━━━━━━━━━━━━━━━\n"
        f"👤 <b>نام فرستنده:</b> {safe_name}\n"
        f"📧 <b>ایمیل:</b> <code>{safe_email}</code>\n"
        f"📱 <b>شماره تماس:</b> <code>{safe_phone}</code>\n"
        f"✈️ <b>تلگرام:</b> {safe_telegram}\n\n"
        "💬 <b>متن پیام:</b>\n"
        f"<blockquote>{safe_message}</blockquote>\n"
        "━━━━━━━━━━━━━━━━━━\n"
        f"⏱ <i>زمان ثبت: {now_str}</i>"
    )


@app.get("/")
@app.get("/health")
async def health_check():
    """Health check endpoint exposing microservice status and monitored infrastructure status."""
    all_healthy = all(svc["is_healthy"] for svc in service_states.values())
    return {
        "status": "healthy" if all_healthy else "degraded",
        "service": "telegram-contact-and-alert-daemon",
        "bot_configured": bool(TELEGRAM_BOT_TOKEN and TELEGRAM_CHAT_ID),
        "target_admin_chat_id": TELEGRAM_ADMIN_CHAT_ID,
        "monitored_services": service_states,
        "timestamp": datetime.now().isoformat()
    }


@app.post("/contact-webhook", status_code=status.HTTP_200_OK)
async def handle_contact_webhook(payload: ContactRequest):
    """Receive contact form payload and dispatch to Telegram admin chat."""
    if not bot or not TELEGRAM_CHAT_ID:
        logger.error("Telegram bot is not configured on the server")
        raise HTTPException(
            status_code=status.HTTP_503_SERVICE_UNAVAILABLE,
            detail="سرویس ربات تلگرام در حال حاضر پیکربندی نشده است."
        )

    formatted_text = format_telegram_message(payload)

    try:
        logger.info("Forwarding contact message from '%s' (%s) to Telegram chat %s", payload.name, payload.email, TELEGRAM_CHAT_ID)
        await bot.send_message(
            chat_id=TELEGRAM_CHAT_ID,
            text=formatted_text,
            parse_mode=ParseMode.HTML,
            disable_web_page_preview=True
        )
        return {
            "success": True,
            "message": "پیام شما با موفقیت دریافت و ارسال گردید."
        }
    except TelegramError as te:
        logger.error("Telegram API error occurred: %s", str(te), exc_info=True)
        raise HTTPException(
            status_code=status.HTTP_502_BAD_GATEWAY,
            detail=f"خطا در ارتباط با سرور تلگرام: {str(te)}"
        )
    except Exception as exc:
        logger.error("Unexpected error handling webhook: %s", str(exc), exc_info=True)
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail="خطای داخلی در ارسال پیام به سرور."
        )


if __name__ == "__main__":
    import uvicorn
    port = int(os.getenv("PORT", 8000))
    host = os.getenv("HOST", "0.0.0.0")
    logger.info("Starting Telegram Bot microservice on %s:%s", host, port)
    uvicorn.run("main:app", host=host, port=port, reload=True)
