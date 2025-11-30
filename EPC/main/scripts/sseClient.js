/**
 * SSE Client Helper - Replaces WebSocket functionality with Server-Sent Events
 */
class SSEClient {
  constructor(endpoint) {
    this.endpoint = endpoint
    this.eventSource = null
    this.onmessage = null
    this.onopen = null
    this.onclose = null
    this.onerror = null
    this.reconnectAttempts = 0
    this.maxReconnectAttempts = 5
    this.reconnectDelay = 1000
  }

  connect() {
    try {
      this.eventSource = new EventSource(`/sse/${this.endpoint}`)

      this.eventSource.onopen = (event) => {
        this.reconnectAttempts = 0
        if (this.onopen) this.onopen(event)
      }

      this.eventSource.addEventListener(this.endpoint, (event) => {
        if (this.onmessage) {
          this.onmessage({ data: event.data })
        }
      })

      this.eventSource.onerror = (event) => {
        if (this.eventSource.readyState === EventSource.CLOSED) {
          if (this.onclose) this.onclose(event)
          this.attemptReconnect()
        }
        if (this.onerror) this.onerror(event)
      }
    } catch (error) {
      console.error('SSE connection error:', error)
      if (this.onerror) this.onerror(error)
    }
  }

  attemptReconnect() {
    if (this.reconnectAttempts < this.maxReconnectAttempts) {
      this.reconnectAttempts++
      setTimeout(() => {
        console.log(
          `Attempting to reconnect (${this.reconnectAttempts}/${this.maxReconnectAttempts})...`
        )
        this.connect()
      }, this.reconnectDelay * this.reconnectAttempts)
    }
  }

  close() {
    if (this.eventSource) {
      this.eventSource.close()
      this.eventSource = null
    }
  }
}
