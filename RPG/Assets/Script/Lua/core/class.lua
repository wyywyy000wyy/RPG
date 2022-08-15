function class(pre, base)
    local c = T[pre] or {} -- a new class instance
    T[pre] = c
    c.__class_name = pre
    c._base = base
    c.__index = c
    c._bases = base and table.copy(base._bases) or {}
    local _ = base and table.index(c._bases, base)

    -- expose a constructor which can be called by <classname>(<args>)
    local mt = pre and getmetatable(pre) or {}

    mt.__call = function(class_tbl, ...)
        local obj = {}
        setmetatable(obj, c)
        for i = #c._bases, 1 do
            local pc = c._bases[i]
            local _ = pc.constructor and pc.constructor(obj, ...)
        end
        local _ = c.constructor and c.constructor(obj, ...)
        return obj
    end

    c.is_a = function(self, klass)
        local m = getmetatable(self)
        while m do
            if m == klass then
                return true
            end
            m = m._base
        end
        return false
    end
    setmetatable(c, mt)
    return c
end