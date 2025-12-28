"""
Pydantic models for Demand Forecasting API responses
"""
from pydantic import BaseModel, Field
from typing import Optional


class DemandForecastResponse(BaseModel):
    """Response model for Demand Forecasting API"""
    
    predicted_units_sold: float = Field(..., description="Predicted units sold")
    record_id: Optional[int] = Field(None, description="Record ID if provided")
    strategy_used: str = Field(..., description="Strategy used for prediction")
    status: str = Field("success", description="Status of the request")
    
    class Config:
        json_schema_extra = {
            "example": {
                "predicted_units_sold": 21.16,
                "record_id": None,
                "strategy_used": "lightgbm",
                "status": "success"
            }
        }

