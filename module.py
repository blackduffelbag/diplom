import socket
import numpy as np
import encodings
import time
from time import sleep
import math
import smbus


PWR_MGMT_1   = 0x6B
SMPLRT_DIV   = 0x19
CONFIG       = 0x1A
GYRO_CONFIG  = 0x1B
ACCEL_CONFIG = 0x1C
INT_ENABLE   = 0x38
ACCEL_XOUT_H = 0x3B
ACCEL_YOUT_H = 0x3D
ACCEL_ZOUT_H = 0x3F
GYRO_XOUT_H  = 0x43
GYRO_YOUT_H  = 0x45
GYRO_ZOUT_H  = 0x47
  
Device_Address = 0x68
multiplexer_address = 0x70
                                                                                
I2C_ch = [0B00000001,
          0B00000010,
          0B00000100,
          0B00001000,
          0B00010000,
          0B00100000,
          0B01000000,
          0B10000000]
 
gyroOffsetX={}
gyroOffsetY={}
gyroOffsetZ={}

angX={}
angY={}
angZ={}


ip='192.168.1.107'
port=5005

bus = smbus.SMBus(1)

buffer=20
Module_Quantity = 6
interval= 0
preinterval=time.time()


def MPU_Init():
    bus.write_byte_data(Device_Address, SMPLRT_DIV, 0x07)
    
    bus.write_byte_data(Device_Address, PWR_MGMT_1, 0x01)
    
    bus.write_byte_data(Device_Address, CONFIG, 0x0)
    
    bus.write_byte_data(Device_Address, GYRO_CONFIG, 0x08)
    
    bus.write_byte_data(Device_Address, ACCEL_CONFIG, 0x00)
    
    bus.write_byte_data(Device_Address, INT_ENABLE, 0x01)

def read_raw_data(addr):
    high = bus.read_byte_data(Device_Address, addr)
    low = bus.read_byte_data(Device_Address, addr+1)
    
    value = ((high << 8) | low)
        
    if(value > 32768):
        value = value - 65536
    return value
    
def dist(a,b):
    return math.sqrt((a*a)+(b*b))

def calibration(n):
    Gx=0
    Gy=0
    Gz=0
    for i in range(3000):
        gyro_x = read_raw_data(GYRO_XOUT_H)
        gyro_y = read_raw_data(GYRO_YOUT_H)
        gyro_z = read_raw_data(GYRO_ZOUT_H)
    
        Gx += gyro_x/65.5
        Gy += gyro_y/65.5
        Gz += gyro_z/65.5
        
        print(i)
    gyroOffsetX[n]=Gx/3000
    gyroOffsetY[n]=Gy/3000
    gyroOffsetZ[n]=Gz/3000

    


for n in range(2):
    bus.write_byte(multiplexer_address, I2C_ch[n])
    MPU_Init()
    calibration(n)
    angX[n]=0
    angY[n]=0
    angZ[n]=0
    
    
print(gyroOffsetX,gyroOffsetY,gyroOffsetZ)



s=socket.socket(socket.AF_INET, socket.SOCK_STREAM)
s.bind((ip, port))
s.listen(1)
print("Server Started waiting for client to connect ")
conn, addr = s.accept()
print('Connnection address: ', addr)
i=0
j=0
while 1:
    for n in range(2):
        print(n)
        bus.write_byte(multiplexer_address, I2C_ch[n])
       
        acc_x = read_raw_data(ACCEL_XOUT_H)
        acc_y = read_raw_data(ACCEL_YOUT_H)
        acc_z = read_raw_data(ACCEL_ZOUT_H)
    
        gyro_x = read_raw_data(GYRO_XOUT_H)
        gyro_y = read_raw_data(GYRO_YOUT_H)
        gyro_z = read_raw_data(GYRO_ZOUT_H)
    
        Ax = acc_x/16384.0
        Ay = acc_y/16384.0
        Az = acc_z/16384.0
    
        Gx = gyro_x/65.5
        Gy = gyro_y/65.5
        Gz = gyro_z/65.5
    
        Gx-=gyroOffsetX[n]
        Gy-=gyroOffsetY[n]
        Gz-=gyroOffsetZ[n]
        
        radians = math.atan2(Ax, dist(Ay,Az))
        y_rot=-math.degrees(radians)
       
        radians = math.atan2(Ay, dist(Ax,Az))
        x_rot=math.degrees(radians)
        
        
        interval= time.time()-preinterval
        angX[n] = 0.96*(angX[n]+Gx*interval)+0.04*x_rot
        angY[n] = 0.96*(angY[n]+Gy*interval)+0.04*y_rot
        angZ[n] += Gz*interval
        preinterval=time.time()
    
        my_data = "{},{},{},{};".format(n,angX[n],angY[n],angZ[n])
        print(my_data)
        if (n==0):
            j+=1
        print("Per model: ", j)
        i+=1
        print("Total: ", i)
        x_encoded_data = my_data.encode('utf-8')
        conn.sendall(x_encoded_data)
    
conn.close()

