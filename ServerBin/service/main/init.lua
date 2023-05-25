
local serviceId

function OnInit(id)
    serviceId = id
    engine.NewService("gateway")
    engine.NewService("login")
end

function OnExit()
    print("[lua] main OnExit")
end



