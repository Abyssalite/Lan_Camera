#!/bin/bash

set -e

VM_DIR="$HOME/vms/ubuntu20"

exec qemu-system-x86_64 \
  -enable-kvm \
  -daemonize \
  -pidfile "$VM_DIR/qemu.pid" \
  -D "$VM_DIR/qemu.log" \
  -cpu host \
  -smp 1 \
  -m 1024 \
  -drive "file=$VM_DIR/ubuntu20.qcow2,format=qcow2,if=virtio" \
  -netdev tap,id=net0,ifname=tap0,script=no,downscript=no \
  -device virtio-net-pci,netdev=net0 \
  -device qemu-xhci \
  -device usb-host,vendorid=0x045e,productid=0x00f5 \
  -display none \
  -serial "file:$VM_DIR/serial.log" \
  -monitor none
  
#  -display gtk    
#  -serial mon:stdio
#  -nic user,model=virtio
