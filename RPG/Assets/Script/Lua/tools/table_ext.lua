function table.copy(tb)
    if type(tb) ~= "table" then
        return nil
    end
    local copy = {}
    for k, v in pairs(tb) do
        copy[k] = v
    end
    return copy
end

function table.deep_copy(tb)
    if type(tb) ~= "table" then
        return nil
    end
    local copy = {}
    for k, v in pairs(tb) do
        if type(v) == "table" then
            copy[k] = table.clone(v, true)
        else
            copy[k] = v
        end
    end
    setmetatable(copy, table.clone(getmetatable(tb), true))
    return copy
end