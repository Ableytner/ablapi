"""Module containing tests for the hello-world module"""

from abllib import log
from flask.testing import FlaskClient

logger = log.get_logger("pylint")

def test_healthz(client: FlaskClient):
    """Checks if /hello-world is reachable"""

    result = client.get("/hello-world")

    assert result.status_code == 200
    assert "Hello World!" in result.text
