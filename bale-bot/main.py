import asyncio
import contextlib
import logging
import os
import re
from datetime import datetime
from typing import Any, Dict, Optional

import httpx
from dotenv import load_dotenv
from fastapi import FastAPI, HTTPException, status
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel, ConfigDict, Field

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s [%(levelname)s] %(name)s: %(message)s"
)
logger = logging.getLogger("bale_webhook_service")

# Load environment variables
load_dotenv()

BALE_BOT_TOKEN = os.getenv("BALE_BOT_TOKEN")
BALE_CHAT_ID = os.getenv("BALE_CHAT_ID")
BALE_ADMIN_CHAT_ID = os.getenv("BALE_ADMIN_CHAT_ID") or BALE_CHAT_ID
BALE_API_BASE = os.getenv("BALE_API_BASE", "https://tapi.bale.ai").rstrip("/")
ALLOWED_ORIGINS_STR = os.getenv("ALLOWED_ORIGINS", "http://localhost:4500,http://127.0.0.1:4500,http://localhost:3000")
ALLOWED_ORIGINS = [origin.strip() for origin in ALLOWED_ORIGINS_STR.split(",") if origin.strip()]

# Monitoring endpoints
BACKEND_HEALTH_URL = os.getenv("BACKEND_HEALTH_URL", "http://backend:5000/health")
FRONTEND_HEALTH_URL = os.getenv("FRONTEND_HEALTH_URL", "http://frontend:4500/")
HEALTH_CHECK_INTERVAL_SECONDS = int(os.getenv("HEALTH_CHECK_INTERVAL_SECONDS", "120"))

if not BALE_BOT_TOKEN or not BALE_CHAT_ID:
    logger.warning("BALE_BOT_TOKEN or BALE_CHAT_ID is not configured properly in environment")


class BaleApiError(Exception):
    """Raised when the Bale Bot API rejects a request or is unreachable."""


async def send_bale_message(chat_id: str, text: str) -> None:
    """Sends a Markdown-formatted text message through the Bale Bot API (Telegram-compatible sendMessage)."""
    if not BALE_BOT_TOKEN:
        raise BaleApiError("BALE_BOT_TOKEN is not configured")

    url = f"{BALE_API_BASE}/bot{BALE_BOT_TOKEN}/sendMessage"
    try:
        async with httpx.AsyncClient(timeout=15.0) as client:
            response = await client.post(url, json={"chat_id": chat_id, "text": text})
    except httpx.HTTPError as exc:
        # Never include the request URL in the error: it contains the bot token
        raise BaleApiError(type(exc).__name__) from None

    try:
        body = response.json()
    except ValueError:
        body = {}

    if response.status_code != 200 or not body.get("ok", False):
        description = body.get("description") or f"HTTP {response.status_code}"
        raise BaleApiError(description)


_MARKDOWN_SPECIALS = re.compile(r"[*`\[\]]")
_SAFE_EMAIL = re.compile(r"^[^\s()\[\]<>]+@[^\s()\[\]<>]+$")


def md_escape(value: str) -> str:
    """Neutralises Bale Markdown control characters in user-supplied text."""
    # Underscores become a fullwidth look-alike so names and addresses stay readable without toggling italics
    return _MARKDOWN_SPECIALS.sub(" ", value).replace("_", "＿")


def md_email(value: str) -> str:
    """Renders an email as a mailto link so the exact address survives Markdown escaping."""
    email = value.strip()
    if _SAFE_EMAIL.match(email):
        return f"[{md_escape(email)}](mailto:{email})"
    return md_escape(email)

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


async def send_emergency_bale_alert(service_name: str, url: str, error_msg: str, failure_count: int) -> None:
    """Dispatches high-priority emergency failure alerts to the Bale admin chat."""
    if not BALE_BOT_TOKEN or not BALE_ADMIN_CHAT_ID:
        logger.error("Cannot send emergency alert: Bale bot token or admin chat ID is missing")
        return

    now_str = datetime.now().strftime("%Y-%m-%d %H:%M:%S")

    alert_text = (
        "🚨 *هشدار اضطراری زیرساخت: قطع یا اختلال در سرویس*\n"
        "━━━━━━━━━━━━━━━━━━\n"
        f"🌐 *سرویس آسیب‌دیده:* {md_escape(service_name)}\n"
        f"🔗 *آدرس بررسی:* {md_escape(url)}\n"
        f"⚠️ *خطای دریافتی:* {md_escape(error_msg)}\n"
        f"🔢 *تعداد خطاهای متوالی:* {failure_count}\n"
        "━━━━━━━━━━━━━━━━━━\n"
        f"⏱ _زمان ثبت رخداد: {now_str}_\n"
        "_لطفاً وضعیت کانتینرها و لاگ‌های سرور را بررسی فرمایید._"
    )

    try:
        await send_bale_message(BALE_ADMIN_CHAT_ID, alert_text)
        logger.info("Dispatched emergency alert for %s to chat %s", service_name, BALE_ADMIN_CHAT_ID)
    except Exception as exc:
        logger.error("Failed to send emergency Bale alert: %s", str(exc))


async def send_recovery_bale_alert(service_name: str, url: str) -> None:
    """Dispatches recovery notification to the Bale admin when a service returns to a healthy state."""
    if not BALE_BOT_TOKEN or not BALE_ADMIN_CHAT_ID:
        return

    now_str = datetime.now().strftime("%Y-%m-%d %H:%M:%S")

    recovery_text = (
        "✅ *اطلاعیه بازیابی سرویس: وضعیت پایدار شد*\n"
        "━━━━━━━━━━━━━━━━━━\n"
        f"🌐 *سرویس:* {md_escape(service_name)}\n"
        f"🔗 *آدرس بررسی:* {md_escape(url)}\n"
        "📊 *وضعیت کنونی:* پایدار (200 OK)\n"
        "━━━━━━━━━━━━━━━━━━\n"
        f"⏱ _زمان بازیابی: {now_str}_"
    )

    try:
        await send_bale_message(BALE_ADMIN_CHAT_ID, recovery_text)
        logger.info("Dispatched recovery notification for %s to chat %s", service_name, BALE_ADMIN_CHAT_ID)
    except Exception as exc:
        logger.error("Failed to send recovery Bale alert: %s", str(exc))


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
                await send_recovery_bale_alert(service_name, url)
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
                await send_emergency_bale_alert(service_name, url, err_msg, config["failure_count"])

    except (httpx.ConnectError, httpx.TimeoutException, httpx.NetworkError, Exception) as exc:
        err_msg = f"{type(exc).__name__}: {str(exc) or 'Connection failed / Timeout'}"
        config["failure_count"] += 1
        config["last_error"] = err_msg
        logger.error("Health check failed for '%s' (%s): %s", service_name, url, err_msg)

        # Alert on first failure or periodic reminders
        if config["is_healthy"] or config["failure_count"] % 5 == 1:
            config["is_healthy"] = False
            await send_emergency_bale_alert(service_name, url, err_msg, config["failure_count"])


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
    logger.info("Starting up Bale Bot & Alert Microservice...")
    monitor_task = asyncio.create_task(health_monitor_daemon())
    yield
    logger.info("Shutting down Bale Bot microservice...")
    monitor_task.cancel()
    try:
        await monitor_task
    except asyncio.CancelledError:
        pass


# Initialize FastAPI App with Lifespan
app = FastAPI(
    title="ShafieiAbadi Bale Bot & Health Daemon",
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
    bale_id: Optional[str] = Field(default=None, description="Sender's Bale username", validation_alias="baleId")

    model_config = ConfigDict(
        populate_by_name=True,
        extra="ignore"
    )


def format_bale_message(payload: ContactRequest) -> str:
    """Safely format contact submission into Bale Markdown."""
    safe_name = md_escape(payload.name.strip())
    safe_email = md_email(payload.email)
    safe_message = md_escape(payload.message.strip())
    safe_phone = md_escape(payload.phone.strip()) if payload.phone and payload.phone.strip() else "ثبت نشده"

    raw_bale = payload.bale_id.strip() if payload.bale_id else ""
    clean_handle = re.sub(r"[^A-Za-z0-9_]", "", raw_bale.lstrip("@"))
    if clean_handle:
        safe_bale = f"[@{clean_handle}](https://ble.ir/{clean_handle})"
    else:
        safe_bale = "ثبت نشده"

    now_str = datetime.now().strftime("%Y-%m-%d %H:%M:%S")

    return (
        "📬 *پیام جدید از فرم تماس وب‌سایت*\n"
        "━━━━━━━━━━━━━━━━━━\n"
        f"👤 *نام فرستنده:* {safe_name}\n"
        f"📧 *ایمیل:* {safe_email}\n"
        f"📱 *شماره تماس:* {safe_phone}\n"
        f"💬 *آیدی بله:* {safe_bale}\n\n"
        "📝 *متن پیام:*\n"
        f"{safe_message}\n"
        "━━━━━━━━━━━━━━━━━━\n"
        f"⏱ _زمان ثبت: {now_str}_"
    )


@app.get("/")
@app.get("/health")
async def health_check():
    """Health check endpoint exposing microservice status and monitored infrastructure status."""
    all_healthy = all(svc["is_healthy"] for svc in service_states.values())
    return {
        "status": "healthy" if all_healthy else "degraded",
        "service": "bale-contact-and-alert-daemon",
        "bot_configured": bool(BALE_BOT_TOKEN and BALE_CHAT_ID),
        "target_admin_chat_id": BALE_ADMIN_CHAT_ID,
        "monitored_services": service_states,
        "timestamp": datetime.now().isoformat()
    }


@app.post("/contact-webhook", status_code=status.HTTP_200_OK)
async def handle_contact_webhook(payload: ContactRequest):
    """Receive contact form payload and dispatch to the Bale admin chat."""
    if not BALE_BOT_TOKEN or not BALE_CHAT_ID:
        logger.error("Bale bot is not configured on the server")
        raise HTTPException(
            status_code=status.HTTP_503_SERVICE_UNAVAILABLE,
            detail="سرویس ربات بله در حال حاضر پیکربندی نشده است."
        )

    formatted_text = format_bale_message(payload)

    try:
        logger.info("Forwarding contact message from '%s' (%s) to Bale chat %s", payload.name, payload.email, BALE_CHAT_ID)
        await send_bale_message(BALE_CHAT_ID, formatted_text)
        return {
            "success": True,
            "message": "پیام شما با موفقیت دریافت و ارسال گردید."
        }
    except BaleApiError as be:
        logger.error("Bale API error occurred: %s", str(be))
        raise HTTPException(
            status_code=status.HTTP_502_BAD_GATEWAY,
            detail="خطا در ارتباط با سرور بله. لطفاً دوباره تلاش کنید."
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
    logger.info("Starting Bale Bot microservice on %s:%s", host, port)
    uvicorn.run("main:app", host=host, port=port, reload=True)
