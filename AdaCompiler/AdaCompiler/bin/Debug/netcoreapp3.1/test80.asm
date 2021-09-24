       .model small                   
       .586                   
       .stack 100h                   
       .data                   
       a dw ?                   
       b dw ?                   
       d dw ?                   
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
                              
       one proc                   
       push bp                   
       mov bp, sp                   
       sub sp, 10                   
       mov ax, 5                   
       mov a, ax                   
       mov ax, 10                   
       mov b, ax                   
       mov ax, a                   
       mov bx, b                   
       imul bx                   
       add sp, 10                   
       pop bp                   
       ret 0                   
       one endp                   
                              
       main PROC                   
       call one               
       main endp                   
       END start                   
