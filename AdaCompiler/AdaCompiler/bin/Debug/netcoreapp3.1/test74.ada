procedure four is
 a,b:integer;
 procedure one is
  c,d:integer;
 begin
   a:= 5;
   b:= 10;
   d:= 20;
   c:= d + a * b;
 end one;
begin
  one();
end four;
