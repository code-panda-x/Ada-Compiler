       .model small                   
       .586                   
       .stack 100h                   
       .data                   
       .code                   
       include io.asm                   
start  PROC                   
       mov ax, @data                   
       mov ds, ax                   
       call one               
       mov ah, 4ch                   
       mov al,0                   
       int 21h                   
start  ENDP                   
                              
       mov  ax,    5          
       mov  ax,    10         
                              
       main PROC                   
       call one               
       main endp                   
       END start                   
