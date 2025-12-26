"""
Pydantic models for Demand Forecasting API requests
"""
from pydantic import BaseModel, Field
from typing import Optional


class DemandForecastRequest(BaseModel):
    """Request model for Demand Forecasting API"""
    
    week: str = Field(..., description="Week in DD/MM/YY format (e.g., '17/01/11')")
    store_id: int = Field(..., description="Store ID")
    sku_id: int = Field(..., description="SKU ID")
    base_price: float = Field(..., gt=0, description="Base price")
    total_price: Optional[float] = Field(None, gt=0, description="Total price (optional, will use base_price if missing)")
    is_featured_sku: int = Field(0, ge=0, le=1, description="Is featured SKU (0 or 1)")
    is_display_sku: int = Field(0, ge=0, le=1, description="Is display SKU (0 or 1)")
    strategy: str = Field("lightgbm", description="Strategy to use for prediction (default: lightgbm)")
    
    class Config:
        json_schema_extra = {
            "example": {
                "week": "17/01/11",
                "store_id": 8091,
                "sku_id": 216418,
                "base_price": 111.8625,
                "total_price": 99.0375,
                "is_featured_sku": 0,
                "is_display_sku": 0,
                "strategy": "lightgbm"
            }
        }

