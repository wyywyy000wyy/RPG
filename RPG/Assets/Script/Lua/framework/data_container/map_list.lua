T.map_list = class("map_list", T.container_base)
local map_list = T.map_list

function map_list:constructor()
    self._map = {}
    self._list = {}
end

function map_list:add(key, value)
    if self:contain(key) then
        ERROR("map_list:add dup key", key, value)
        return
    end
    table.insert(self._list, value)
    self._map[key] = #self._list
end

function map_list:remove(key)
    local idx = self._map[key]
    if not idx then
        return
    end
    self._map[key] = nil
    self._list[idx] = nil
end

function map_list:contain(key)
    return self._map[key] ~= nil
end

function map_list:get(key)
    local idx = self._map[key]
    return idx and self._list[idx]
end

function map_list:set(key, value)
    local idx = self._map[key]
    if idx then
        self._list[idx] = value
        return
    end 
    self:add(key, value)
end