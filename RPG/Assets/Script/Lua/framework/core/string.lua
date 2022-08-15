function string.split(str, delimiter)
    if str == nil or str == "" or delimiter == nil then
        return nil
    end

    local results = {}
    for match in (str ..delimiter):gmatch("(.-)"..delimiter) do
        table.insert(results, match)
    end
    return results
end

function string.splitDot(str)
    if str == nil or str == ""then
        return nil
    end

    local results = {}
    for match in (str ..'.'):gmatch("(.-)"..'%.') do
        table.insert(results, match)
    end
    return results
end

function string.splitDot2(str)
    if str == nil or str == "" then
        return nil
    end

    local results = {}

    local s = 1
    local l = 0

    for i = 1, string.len(str) do
        if str[i] ~= '.' then
            -- s = i
            l = l + 1
        else
            local tstr = string.sub(str, s, s + l) or ""
            table.insert(results, tstr)
            s = i + 1
            l = 0
        end
    end

    if l > 0 then
        local tstr = string.sub(str, s, s + l) or ""
        table.insert(results, tstr)
    end

    return results
end

function string.join(arr, delimiter)
    local s=""
    local len=#arr
    for i,v in ipairs(arr) do
        s=s..v
        if i<len then s=s..delimiter end
    end
    return s
end

function string.utf8_len(s)
    local _, count = string.gsub(s, "[^\128-\193]", "")
    return count
end

---按照给定的长度对传入的字符串做字节长度检测，返回布尔值：可用与否
function is_inputlen_avaliable(a_str,a_cfg_len)
    if not a_str then return false end
    if not a_cfg_len then return true end
    return #a_str >= a_cfg_len[1] and #a_str <= a_cfg_len[2]
end

function string.contains_sign(str,...)
    local signs = {...}
    for k,v in pairs(signs or {}) do
        if string.match(str,v) then
            return true
        end
    end
    return false
end
