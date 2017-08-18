
local this = {}



function this.Update()
	
end

function this.LateUpdate()

end

function this.FixedUpdate()

end


function decode(msg)
	local mtype = msg:UnmarshalInt()
	print("=msg type", mtype)
	local o = {}
	o.x = msg:UnmarshalInt()
	o.y = msg:UnmarshalLong()
	o.a = msg:UnmarshalBool()
	o.b = msg:UnmarshalString()
	o.c = msg:UnmarshalBinary()
	o.f = msg:UnmarshalFloat()
	
	o.m = msg:UnmarshalDouble()
	
	o.li = {}
	local n = msg:UnmarshalSize()
	for i = 1, n do
		table.insert(o.li, msg:UnmarshalInt())
	end
	o.ll = {}
	local n = msg:UnmarshalSize()
	for i = 1, n do
		table.insert(o.ll, msg:UnmarshalLong())
	end
	for k,v in pairs(o) do
		print("==msg.field", k, v)
	end
	return o
end

function encode(msg)
	local ds = Aio.BinaryStream()
	ds:MarshalInt(65179)
	ds:MarshalInt(msg.x)
	ds:MarshalLong(msg.y)
	ds:MarshalBool(msg.a)
	ds:MarshalString(msg.b)
	ds:MarshalBinary(msg.c)
	ds:MarshalFloat(msg.f)
	ds:MarshalDouble(msg.m)
	
	ds:MarshalSize(#(msg.li))
	for _, v in ipairs(msg.li) do
		ds:MarshalInt(v)
	end
	
	ds:MarshalSize(#(msg.ll))
	for _, v in ipairs(msg.ll) do
		ds:MarshalLong(v)
	end
	
	return ds:toArray()
end

function this.RecvMsg(msg)
	print("== recv", msg)
	NetManager.Ins:Send(encode(decode(msg)))
end

function main()

	print("hello")
	NetManager.Ins:Connect("localhost", 1218)
	return this
end