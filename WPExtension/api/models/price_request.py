"""
Pydantic models for Price Forecasting API requests
"""
from pydantic import BaseModel, Field
from typing import Optional
from datetime import date


class PriceForecastRequest(BaseModel):
    """Request model for Price Forecasting API"""
    
    Store: int = Field(..., ge=1, description="Store ID (1-45)")
    Dept: int = Field(..., ge=1, description="Department ID (1-99)")
    Date: str = Field(..., description="Date in YYYY-MM-DD format")
    IsHoliday: Optional[bool] = Field(None, description="Is holiday week (optional, will be looked up if missing)")
    strategy: str = Field("linear", description="Strategy to use for prediction (default: linear, options: linear, dnn)")
    
    class Config:
        json_schema_extra = {
            "example": {
                "Store": 1,
                "Dept": 1,
                "Date": "2012-11-02",
                "IsHoliday": False,
                "strategy": "linear"
            }
        }

