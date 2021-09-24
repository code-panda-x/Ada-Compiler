procedure two is
 a,b:integer;
  procedure x is 
  cc,d:integer;
  begin
    put("Enter a number ");
    get(a);
    b:=10;
    d:=20;
    cc:=d+a*b;
    put("The answer is ");
    putln(cc);
  end x;
begin
  x();
end two;
