import requests

from info import URL


def test_user_register():
    credentials = {
        "name": "John Doe",
        "password": "password",
    }
    response = requests.post(
        URL + "/register",
        json=credentials,
    )
    assert response.status_code == 200
    assert response.json() == {"status": "success"}