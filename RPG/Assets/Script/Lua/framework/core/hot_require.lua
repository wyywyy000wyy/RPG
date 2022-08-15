-- module("hot_require", package.seeall)

hot_require = {}

local get_file_name --= CS.DrawTest.GetFileName
local get_file_time = function(path)
    local sysTime =E.File.GetLastWriteTime(path);
    return sysTime.Ticks;
end--CS.DrawTest.FileWriteTime

local is_editor = CS.UnityEngine.Application.platform == CS.UnityEngine.RuntimePlatform.WindowsEditor

function get_file_name(path)
    local tpath = string.gsub(path, "%.", "/")  .. ".lua"
    if is_editor then
        return "Assets/Lua/" .. tpath
    end
    -- local file = io.open(CS.DrawTest.ExternPath .. tpath)
    -- if file then
    --     io
    -- end

    return CS.DrawTest.ExternPath .. tpath
end

local prequire = require
local load_map = {}
local init_map = {}
local VMConfig

local function config_name(modname)
    local arr = string.splitDot(modname)
    return arr[#arr]
end

require = function(modname)
    local require_info = init_map[modname]
    if not require_info then
        local filepath = get_file_name(modname)
        local filetime = get_file_time(filepath)
        -- local is_vm = is_vm(modname, filepath)
        local tmap = load_map
        require_info = {
            name = config_name(modname),
            modname = modname,
            filetime = filetime,
        }
        tmap[filepath] = require_info
        init_map[modname] = require_info
    end
    -- is_vm = is_vm,
    -- vm_name = is_vm and config_name(modname),
    return prequire(modname)
end

function hot_require._rrquire_all(is_config)
    local tmap = load_map
    for filepath, info in pairs(tmap) do
        local filetime = info.filetime
        local cur = get_file_time(filepath)
        -- logger.print(filepath .. " " .. tostring(filetime) .. " cur" .. tostring(cur))
        if cur > filetime and package.loaded[info.modname] then
            logger.print("GGGGGGGG....... 热更了文件：" .. filepath)
            dofile(filepath)
            info.filetime = cur
        end
        ::continue::
    end
end

local normal_require = require
local check_require = function(modname)
    local info = init_map[modname]
    if info then
        local filepath = info.filepath
        local filetime = info.filetime
        local cur = get_file_time(filepath)
        if cur > filetime then
            logger.print("GGGGGGGG....... 热更了文件：" .. filepath)
            if package.loaded[info.modname] then
                dofile(filepath)
            else
                prequire(modname)
            end
            info.filetime = cur
        end
    else
        normal_require(modname)
    end
end

function hot_require.rrquire_all()
    logger.print("GGGGGGGG....... F5 热更")
    hot_require._rrquire_all(true)
end





local ReloadButtonGo = E.GameObject.Find("Canvas/ReloadButton")
if ReloadButtonGo then
    local button = ReloadButtonGo:GetComponent(typeof(E.Button))
    local UnityEvent = E.ButtonClickedEvent()
    UnityEvent:AddListener(function()
        hot_require.rrquire_all()
    end)
    button.onClick = UnityEvent
end