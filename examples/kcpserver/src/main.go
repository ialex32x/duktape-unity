// package main

// import (
// 	"fmt"

// 	"github.com/xtaci/kcp-go"
// )

// type KCPTest struct {
// 	kcp *KCP
// }

// func NewKCPTest(udp *net.UDPConn ,remote *net.UDPAddr)(*KCPTest) {
// 	test := new(KCPTest)
// 	test.kcp = NewKCP(1234, func(buf []byte, size int) {
// 		udp.WriteToUDP(buf[:n], remote)
// 	})
// 	return test
// }

// func main() {
// 	addr, _ := net.ResolveUDPAddr("udp", ":1234")
// 	udp, _ := net.ListenUDP("udp", addr)
// 	reg := make(map[*net.UDPConn]*KCPTest)
// 	for {
// 		data := make([]byte, 1024)
// 		n, remote, err := udp.ReadFromUDP(data)
// 		if err != nil {
// 			fmt.Println(err, remote)
// 			continue
// 		}
// 		if n <= 0 {
// 			fmt.Println("zero data", remote)
// 		}
// 		if n > 0 {
// 			var test *KCPTest
// 			var ok bool
// 			if test, ok = reg[remote]; !ok {
// 				test = NewKCPTest(udp, remote)
// 				reg[remote] = test
// 			}
// 			if retval := test.kcp.Input(data[:n], true, false); retval >= 0 {
// 				test.kcp.Recv()
// 			}

// 			test.kcp.Send(data[:n])
// 		}
// 	}
// }
