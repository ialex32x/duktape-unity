package main

import (
	"fmt"
	"net"
)

func main() {
	addr, _ := net.ResolveUDPAddr("udp", ":1234")
	udp, _ := net.ListenUDP("udp", addr)
	for {
		data := make([]byte, 1024)
		n, remote, err := udp.ReadFromUDP(data)
		if err != nil {
			fmt.Println(err, remote)
			continue
		}
		if n <= 0 {
			fmt.Println("zero data", remote)
		}
		if n > 0 {
			udp.WriteToUDP(data[:n], remote)
		}
	}
}
