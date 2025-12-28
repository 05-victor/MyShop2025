"""
Pydantic models for Price Forecasting API responses
"""
from pydantic import BaseModel, Field


class PriceForecastResponse(BaseModel):
    """Response model for Price Forecasting API"""
    
    predicted_weekly_sales: float = Field(..., description="Predicted weekly sales")
    store: int = Field(..., description="Store ID")
    dept: int = Field(..., description="Department ID")
    date: str = Field(..., description="Date used for prediction")
    strategy_used: str = Field(..., description="Strategy used for prediction")
    status: str = Field("success", description="Status of the request")
    
    class Config:
        json_schema_extra = {
            "example": {
                "predicted_weekly_sales": 9876.54,
                "store": 1,
                "dept": 1,
                "date": "2012-11-02",
                "strategy_used": "linear",
                "status": "success"
            }
        }

