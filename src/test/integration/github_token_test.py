"""Module containing tests to check if the provided Github token is valid"""

import os

import pytest
import requests
from abllib import log

logger = log.get_logger("pylint")

def test_github_token_valid():
    """Checks if the Github token is valid, skips the test if no token is provided"""

    if "GITHUB_TOKEN" not in os.environ:
        pytest.skip("No GITHUB_TOKEN found in environment")

    headers: dict[str, str | bytes] = {
        "Accept": "application/vnd.github+json",
        "Accept-Charset": "UTF-8",
        "Authorization": f"Bearer {os.environ["GITHUB_TOKEN"]}",
        "X-GitHub-Api-Version": "2022-11-28"
    }
    res = requests.get("https://api.github.com/users/Ableytner", headers=headers, timeout=30)

    assert res.ok
