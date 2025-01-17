import requests
import threading
import sys
import time
import traceback
import io

# Force UTF-8 encoding for stdout and stderr
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding='utf-8')

OLLAMA_SERVER_URL = "http://10.0.0.232:11434/api/v1/chat"

def send_prompt(prompt, conversation_id):
    """
    Send a prompt to the Ollama server and return the response.
    """
    try:
        payload = {
            "conversation_id": conversation_id,
            "model": "llama3.2",
            "prompt": prompt
        }
        response = requests.post(OLLAMA_SERVER_URL, json=payload, timeout=30)
        response.raise_for_status()
        return response.json()
    except requests.exceptions.RequestException as e:
        sys.stdout.write(f"\n[Error]: Failed to communicate with Ollama server: {e}\n")
        traceback.print_exc()
        return None

def handle_output(conversation_id, stop_event):
    """
    Continuously poll for responses from the server.
    """
    try:
        while not stop_event.is_set():
            time.sleep(0.1)  # Slight delay to avoid busy-waiting
    except Exception as e:
        sys.stdout.write(f"\n[Error]: An error occurred in handle_output: {e}\n")
        traceback.print_exc()

def main():
    """
    Interact with the Ollama server directly.
    """
    stop_event = threading.Event()
    conversation_id = str(int(time.time()))  # Unique conversation ID

    try:
        # Start output handling in a separate thread (if needed for polling updates)
        output_thread = threading.Thread(target=handle_output, args=(conversation_id, stop_event), daemon=True)
        output_thread.start()

        sys.stdout.write("[Info]: Connected to Ollama server.\n")
        sys.stdout.flush()

        while True:
            # Get user input
            prompt = input("You: ").strip()
            if prompt.lower() == "exit":
                break

            # Send the prompt and receive a response
            sys.stdout.write(f"You: {prompt}\n")
            sys.stdout.flush()
            response = send_prompt(prompt, conversation_id)

            if response and "content" in response:
                sys.stdout.write(f"AI: {response['content']}\n")
                sys.stdout.flush()
            else:
                sys.stdout.write("[Error]: Failed to get a valid response from the server.\n")

    except KeyboardInterrupt:
        sys.stdout.write("\n[Info]: Exiting...\n")
    except Exception as e:
        sys.stdout.write(f"\n[Critical Error]: {e}\n")
        traceback.print_exc()
    finally:
        stop_event.set()
        sys.stdout.write("[Info]: Disconnected from Ollama server.\n")

if __name__ == "__main__":
    main()
