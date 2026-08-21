import requests

SERVER_URL = "https://localhost:44332"
USER_ID = "myid"
USER_TOKEN = "mytoken"

def get_jwt_token(base_url: str, user_id: str, user_token: str) -> str:
	"""Request a new JWT token for the given user ID."""

	url = f"{base_url}/api/Auth/"
	response = requests.post(url, auth=(user_id, user_token), verify=False)
	response.raise_for_status()

	data = response.json()
	token = data["Token"]
	print(f"Received token, expires at {data['ExpiresAt']}")
	return token

def call_hello_world_private(base_url: str, token: str) -> str:
	"""Call the authenticated Hello World endpoint using the JWT token."""

	url = f"{base_url}/api/hello-world/log/"
	headers = {"Authorization": f"Bearer {token}"}
	response = requests.get(url, headers=headers, verify=False)
	response.raise_for_status()
	return response.text

if __name__ == "__main__":
	token = get_jwt_token(SERVER_URL, USER_ID, USER_TOKEN)
	result = call_hello_world_private(SERVER_URL, token)

	print(f"Response from Hello World controller: {result}")
