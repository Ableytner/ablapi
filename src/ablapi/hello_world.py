"""Contains the /hello-world route"""

from ablapi.util import register_endpoint


def hello_world():
    """Return the greetings"""

    return "<h1>Hello World!</p>"

register_endpoint("/hello-world", hello_world)
