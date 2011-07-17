def method_missing(sym, *args)
  if(sym.to_s == "pl")
    # args will be the symbol of the method to invoke, and self is the object
    # on which the method was invoked. The line below sends this object to the
    # method that was missing.
    send(args[0], self)
  end
end

def square ar
  ar.map! { |i| i * i }
end

def halve ar
  ar.map! { |i| i / 2 }
end

(([1,2,3,4,5,6,7,8,9,10].pl :square
    ).pl :halve
    ).pl :puts