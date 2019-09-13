Pro Pooling v2.1

Thank you for using Pro Pooling. 

If you have any thoughts, comments, suggestions or otherwise then please contact us through our website or 
drop me a mail directly on mark_a_hewitt@yahoo.co.uk

Please consider rating this asset on the asset store.

Regards,
Mark Hewitt

For more details please visit: http://www.flipwebapps.com/ 
For tutorials visit: http://www.flipwebapps.com/

- - - - - - - - - -

QUICK START

1. If you have an older version installed:
  1.1. Make a backup of your project
  1.2. Delete the old /FlipWebApps/ProPooling folder to cater for possible conflicts.
2. Check out the demo scenes under /FlipWebApps/ProPooling/_Demo

- - - - - - - - - -

CHANGE LOG

v2.1

	- Poolmanager: Updated editor window allowing for drag and drop of prefabs to create new pools
	- PoolManager: Check for null prefabs
	- Tests: Fixes for warnings when running editor tests.
	- Removed unused EditorList class

v2.0
NOTE: This version contains some API changes and may require minor code changes. See below for details. If upgrading you will likely need to delete the 
old /FlipWebApps/ProPooling folder to cater for possible conflicts.

	Improvements
	- Components: IPoolComponent interface methods now have PoolItem as a parameter. Add this to any custom implementations.
	- Components: Added ReturnToPoolAfterDelay to automatically return pool items to their pool after a specified delay.
	- Components: changed namespace from FlipWebApps.ProPooling.Scripts.Components to ProPooling.Components
	- Demo: Added Auto return to pool after delay demo.
	- Pooling: changed namespace for pooling classed from FlipWebApps.ProPooling.Scripts to ProPooling
	- Pool: Updated Documentation
	- Pool: Added Name property
	- Pool: ID automatically returns Prefab Instance ID
	- Pool: Created pool items have same name as prefab
	- Pool: Pool is no longer declared as a generic type for simplicity. Rename references of Pool<PoolItem> to Pool. See also PoolGeneric.
	- Pool: Added PoolGeneric class that inherits from Pool for use with custom PoolItem derived classes.
	- Pool: IReturnToPool is removed. Just reference Pool instead.
	- PoolManager: Methods are no longer static and must be accessed through PoolManager.Instance.
	- PoolManager: Allow referencing pools by name
	- PoolManager: Allow adding new pools for management
	- PoolManager: Check for pools with missing prefab before setup
	- PoolManager: Added GetFromPool, GetPoolItemFromPool and ReturnToPool methods to indirectly get and return items from / to the different pools.

v1.1
	Improvements
	- New graphical demo showing generics, and delegated pool returns.
	- PoolItem lifecycle methods renamed - prefixed with 'On'
	- Added IReturnToPool interface with reference in PoolItem and ReturnSelf() method.
	- Pool - some items reworked around PoolItem to enable delegated references.
	- Added custom pool inspector.

v1.0
	First public release