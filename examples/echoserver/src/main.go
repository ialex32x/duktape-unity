package main

import (
	"net/http"
	"fmt"
	"time"
	"github.com/gorilla/websocket"
)

func homepageHandler(w http.ResponseWriter, r *http.Request) {
	http.Error(w, "does not exist", 404)
}

func accept(conn *websocket.Conn) {
	tp, msg, err := conn.ReadMessage()
	if err != nil{
		return
	}
	conn.WriteMessage(tp, msg)
}

func main() {
	originChecker := func(r *http.Request) bool {
		return true
	}
	upgrader := websocket.Upgrader{
		CheckOrigin:      originChecker,
		HandshakeTimeout: time.Minute,
	}
	http.HandleFunc("/websocket", func(w http.ResponseWriter, r *http.Request) {
		conn, err := upgrader.Upgrade(w, r, nil)
		if err != nil {
			return
		}
		fmt.Printf("open %v", r.RemoteAddr)
		accept(conn)
	})
	http.HandleFunc("/", homepageHandler)
	http.ListenAndServe(":8080", nil)
}
