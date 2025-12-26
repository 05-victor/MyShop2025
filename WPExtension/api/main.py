"""
Main FastAPI application
"""
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
import logging

from api.handlers import demand_handler, price_handler
from api.strategies.demand.registry import demand_registry
from api.strategies.price.registry import price_registry

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)

# Create FastAPI app
app = FastAPI(
    title="Forecasting APIs",
    description="REST APIs for Demand Forecasting and Price Forecasting",
    version="1.0.0",
    docs_url="/docs",
    redoc_url="/redoc"
)

# CORS middleware to allow calls from C# and other languages
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # In production, specify allowed origins
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Register routers
app.include_router(demand_handler.router)
app.include_router(price_handler.router)


@app.on_event("startup")
async def startup_event():
    """Load all strategies when app starts"""
    logger.info("Starting up...")
    try:
        # Load demand strategies (lazy loading will happen on first use)
        logger.info(f"Registered demand strategies: {demand_registry.list_all()}")
        
        # Load price strategies (lazy loading will happen on first use)
        logger.info(f"Registered price strategies: {price_registry.list_all()}")
        
        logger.info("Startup complete")
    except Exception as e:
        logger.error(f"Error during startup: {str(e)}", exc_info=True)


@app.get("/health")
async def health():
    """Health check endpoint"""
    return {
        "status": "healthy",
        "demand_strategies": demand_registry.list_all(),
        "price_strategies": price_registry.list_all()
    }


@app.get("/strategies")
async def list_strategies():
    """List all available strategies"""
    return {
        "demand": {
            "available": demand_registry.list_all(),
            "default": demand_registry.get_default_name() if hasattr(demand_registry, 'get_default_name') else demand_registry.list_all()[0]
        },
        "price": {
            "available": price_registry.list_all(),
            "default": price_registry.get_default_name() if hasattr(price_registry, 'get_default_name') else price_registry.list_all()[0]
        }
    }


@app.get("/")
async def root():
    """Root endpoint with API information"""
    return {
        "message": "Forecasting APIs",
        "version": "1.0.0",
        "docs": "/docs",
        "health": "/health",
        "strategies": "/strategies"
    }

