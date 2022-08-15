local dbg = _G.emmy_core
if dbg then
    print("dbgdbgdbg" .. (dbg and "true" or "false"))
    local port = 9866
    for i = 0, 100 do
        local ret = xpcall(function()
            port = port + i
            local ret = dbg.tcpListen("localhost", port)
            return true
        end, function(err)
            -- logger.print("dbgdbgdbg LUA ERROR: " .. tostring(err))
            return false
        end)
        if ret then
            print("dbgdbgdbg ret=" .. tostring(port))
            break
        end
    end

end

require("global_require")
require("core.string")
require("core.hot_require")
require("main")


-- require("TestSwitchDraw")
-- require("TestSwitchDraw2")
