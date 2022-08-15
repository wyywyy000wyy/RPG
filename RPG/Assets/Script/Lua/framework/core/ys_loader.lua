
module("ys_loader", package.seeall)

local ip = "10.23.27.57"
local port = "8787"--"5500"
local head = "http://" .. ip .. ":" .. port .."/settings.json"

E.Addressables.InternalIdTransformFunc = function(location)
    local ltype = location.ResourceType 
    local key = location.PrimaryKey
    local path = location.InternalId
    if ltype == typeof(CS.UnityEngine.AddressableAssets.Initialization.ResourceManagerRuntimeData) then
        return head
        
    end

    -- if ltype == typeof(CS.UnityEngine.AddressableAssets.ResourceLocators.ContentCatalogData) 
    
    -- then
    -- end
    print(ltype:ToString())

    -- if ltype == typeof() do
    -- end

    return path
end





local op = E.Addressables.InitializeAsync()
-- op:WaitForCompletion()



-- ys_loader = {}


function LoadAssetAsync(key, func)
    local cop = E.Addressables.CheckForCatalogUpdates(false);

    local list = cop:WaitForCompletion()



    local catalogs = {
        "AddressablesMainContentCatalog"
    }
    local strHash = "d98949e8246afdc05869ee873971eb99"
    -- local estrHash = "d98949e8246afdc05869ee873971eb98"
    local bundleName = "textures_assets_all"
    -- local op = E.Addressables.UpdateCatalogs(catalogs)
    -- op:WaitForCompletion()
    local url = "http://" .. ip .. ":" .. port .."/bundle/" .. bundleName .. "_" .. strHash .. ".bundle"


    -- local hash = CS.UnityEngine.Hash128.Parse(strHash)

    -- local newCache = CS.UnityEngine.Caching.AddCache("F:\\bundle\\");
    -- CS.UnityEngine.Caching.currentCacheForWriting = newCache;

    -- CS.YSLoader._ins:DownloadAndCacheAssetBundle(url, hash, function(bundle)
    --     CS.UnityEngine.Caching.MarkAsUsed(url, hash)
    --     local a = 1
    --     a = bundle
    -- end)

    CS.YSLoader.UpdateCatalogs("AddressablesMainContentCatalog")

    -- local list = E.Addressables.CheckForCatalogUpdates(true)


    local handle = CS.YSLoader.LoadAssetAsync(key, func)
end

-- io.open()
