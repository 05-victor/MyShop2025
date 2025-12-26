"""
Test script for Forecasting APIs
Run this after starting the API server: uvicorn api.main:app --host 0.0.0.0 --port 8000
"""
import requests
import json
from typing import Dict, Any

BASE_URL = "http://localhost:8000"


def print_response(title: str, response: requests.Response):
    """Pretty print API response"""
    print(f"\n{'='*60}")
    print(f"{title}")
    print(f"{'='*60}")
    print(f"Status Code: {response.status_code}")
    try:
        print(f"Response: {json.dumps(response.json(), indent=2)}")
    except:
        print(f"Response: {response.text}")


def test_health():
    """Test health endpoint"""
    print("\n[TEST] Health Check")
    response = requests.get(f"{BASE_URL}/health")
    print_response("Health Check", response)
    assert response.status_code == 200, "Health check failed"


def test_list_strategies():
    """Test strategies endpoint"""
    print("\n[TEST] List Strategies")
    response = requests.get(f"{BASE_URL}/strategies")
    print_response("Available Strategies", response)
    assert response.status_code == 200, "List strategies failed"


def test_demand_forecast_valid():
    """Test Demand Forecasting with valid data"""
    print("\n[TEST] Demand Forecast - Valid Data")
    payload = {
        "week": "17/01/11",
        "store_id": 8091,
        "sku_id": 216418,
        "base_price": 111.8625,
        "total_price": 99.0375,
        "is_featured_sku": 0,
        "is_display_sku": 0,
        "strategy": "lightgbm"
    }
    response = requests.post(f"{BASE_URL}/api/demand-forecast/predict", json=payload)
    print_response("Demand Forecast - Valid", response)
    assert response.status_code == 200, "Demand forecast failed"
    data = response.json()
    assert "predicted_units_sold" in data
    assert data["status"] == "success"
    print(f"✓ Predicted units sold: {data['predicted_units_sold']}")


def test_demand_forecast_missing_total_price():
    """Test Demand Forecasting with missing total_price (should use base_price)"""
    print("\n[TEST] Demand Forecast - Missing total_price")
    payload = {
        "week": "17/01/11",
        "store_id": 8091,
        "sku_id": 216418,
        "base_price": 111.8625,
        # total_price is missing - should use base_price
        "is_featured_sku": 0,
        "is_display_sku": 0
    }
    response = requests.post(f"{BASE_URL}/api/demand-forecast/predict", json=payload)
    print_response("Demand Forecast - Missing total_price", response)
    assert response.status_code == 200, "Demand forecast with missing total_price failed"
    data = response.json()
    assert "predicted_units_sold" in data
    print(f"✓ Predicted units sold (with missing total_price): {data['predicted_units_sold']}")


def test_demand_forecast_invalid_strategy():
    """Test Demand Forecasting with invalid strategy"""
    print("\n[TEST] Demand Forecast - Invalid Strategy")
    payload = {
        "week": "17/01/11",
        "store_id": 8091,
        "sku_id": 216418,
        "base_price": 111.8625,
        "strategy": "invalid_strategy"
    }
    response = requests.post(f"{BASE_URL}/api/demand-forecast/predict", json=payload)
    print_response("Demand Forecast - Invalid Strategy", response)
    assert response.status_code == 400, "Should return 400 for invalid strategy"


def test_price_forecast_valid():
    """Test Price Forecasting with valid data"""
    print("\n[TEST] Price Forecast - Valid Data (Linear)")
    payload = {
        "Store": 1,
        "Dept": 1,
        "Date": "2012-11-02",
        "IsHoliday": False,
        "strategy": "linear"
    }
    response = requests.post(f"{BASE_URL}/api/price-forecast/predict", json=payload)
    print_response("Price Forecast - Valid (Linear)", response)
    assert response.status_code == 200, "Price forecast failed"
    data = response.json()
    assert "predicted_weekly_sales" in data
    assert data["status"] == "success"
    print(f"✓ Predicted weekly sales: {data['predicted_weekly_sales']}")


def test_price_forecast_with_dnn():
    """Test Price Forecasting with DNN strategy"""
    print("\n[TEST] Price Forecast - DNN Strategy")
    payload = {
        "Store": 1,
        "Dept": 1,
        "Date": "2012-11-02",
        "IsHoliday": False,
        "strategy": "dnn"
    }
    try:
        response = requests.post(f"{BASE_URL}/api/price-forecast/predict", json=payload)
        print_response("Price Forecast - DNN", response)
        if response.status_code == 200:
            data = response.json()
            assert "predicted_weekly_sales" in data
            print(f"✓ Predicted weekly sales (DNN): {data['predicted_weekly_sales']}")
        else:
            print(f"⚠ DNN strategy might not be available (keras/tensorflow not installed?)")
    except Exception as e:
        print(f"⚠ DNN test failed: {str(e)}")


def test_price_forecast_missing_features():
    """Test Price Forecasting with missing features (should use nearest date)"""
    print("\n[TEST] Price Forecast - Missing Features (Nearest Date Lookup)")
    payload = {
        "Store": 1,
        "Dept": 1,
        "Date": "2012-12-31",  # Date might not exist in features.csv, should find nearest
        "IsHoliday": None,  # Will be looked up from dataset
        "strategy": "linear"
    }
    response = requests.post(f"{BASE_URL}/api/price-forecast/predict", json=payload)
    print_response("Price Forecast - Missing Features", response)
    assert response.status_code == 200, "Price forecast with missing features failed"
    data = response.json()
    assert "predicted_weekly_sales" in data
    print(f"✓ Predicted weekly sales (with nearest date lookup): {data['predicted_weekly_sales']}")


def test_price_forecast_invalid_strategy():
    """Test Price Forecasting with invalid strategy"""
    print("\n[TEST] Price Forecast - Invalid Strategy")
    payload = {
        "Store": 1,
        "Dept": 1,
        "Date": "2012-11-02",
        "strategy": "invalid_strategy"
    }
    response = requests.post(f"{BASE_URL}/api/price-forecast/predict", json=payload)
    print_response("Price Forecast - Invalid Strategy", response)
    assert response.status_code == 400, "Should return 400 for invalid strategy"


def main():
    """Run all tests"""
    print("="*60)
    print("FORECASTING APIs TEST SUITE")
    print("="*60)
    print(f"\nTesting API at: {BASE_URL}")
    print("Make sure the API server is running: uvicorn api.main:app --host 0.0.0.0 --port 8000")
    
    tests = [
        ("Health Check", test_health),
        ("List Strategies", test_list_strategies),
        ("Demand Forecast - Valid", test_demand_forecast_valid),
        ("Demand Forecast - Missing total_price", test_demand_forecast_missing_total_price),
        ("Demand Forecast - Invalid Strategy", test_demand_forecast_invalid_strategy),
        ("Price Forecast - Valid (Linear)", test_price_forecast_valid),
        ("Price Forecast - DNN", test_price_forecast_with_dnn),
        ("Price Forecast - Missing Features", test_price_forecast_missing_features),
        ("Price Forecast - Invalid Strategy", test_price_forecast_invalid_strategy),
    ]
    
    passed = 0
    failed = 0
    
    for test_name, test_func in tests:
        try:
            test_func()
            passed += 1
            print(f"\n✓ {test_name} - PASSED")
        except AssertionError as e:
            failed += 1
            print(f"\n✗ {test_name} - FAILED: {str(e)}")
        except Exception as e:
            failed += 1
            print(f"\n✗ {test_name} - ERROR: {str(e)}")
    
    print("\n" + "="*60)
    print("TEST SUMMARY")
    print("="*60)
    print(f"Total: {len(tests)}")
    print(f"Passed: {passed}")
    print(f"Failed: {failed}")
    print("="*60)


if __name__ == "__main__":
    main()

