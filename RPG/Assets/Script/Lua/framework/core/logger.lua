

logger = logger or {}
-- Logger = {}

local raw_print = print

function logger.print(...)
    local tab = {...}
    for i, v in ipairs(tab) do
        if type(v) == "table" then
            tab[i] = serpent.block(v, {numformat = "%s", comment = false, maxlevel = MAX_DEPTH})
        end
    end
    -- table.insert(tab, debug.traceback("", 2))
    raw_print("[YSTECH]", table.unpack(tab))
end

function logger.error(...)
    local tab = {...}
    for i, v in ipairs(tab) do
        if type(v) == "table" then
            tab[i] = serpent.block(v, {numformat = "%s", comment = false, maxlevel = MAX_DEPTH})
        end
    end
    -- table.insert(tab, debug.traceback("", 2))
    raw_print("[YSTECH] ERROR", table.unpack(tab))
end

ERROR = logger.error
