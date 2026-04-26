"""Contains the /healthz route"""

from ablapi.util import register_endpoint

def heartbeat():
    """Return a heartbeat"""

    return "OK", 200

register_endpoint("/healthz", heartbeat)
