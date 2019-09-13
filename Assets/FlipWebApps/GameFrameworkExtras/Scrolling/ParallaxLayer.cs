//----------------------------------------------
// Flip Web Apps: Game Framework
// Copyright © 2016 Flip Web Apps / Mark Hewitt
//
// Please direct any bugs/comments/suggestions to http://www.flipwebapps.com
// 
// The copyright owner grants to the end user a non-exclusive, worldwide, and perpetual license to this Asset
// to integrate only as incorporated and embedded components of electronic games and interactive media and 
// distribute such electronic game and interactive media. End user may modify Assets. End user may otherwise 
// not reproduce, distribute, sublicense, rent, lease or lend the Assets. It is emphasized that the end 
// user shall not be entitled to distribute or transfer in any way (including, without, limitation by way of 
// sublicense) the Assets in any other way than as integrated components of electronic games and interactive media. 

// The above copyright notice and this permission notice must not be removed from any files.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//----------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using GameFramework.Debugging;
using GameFramework.EditorExtras;
using GameFramework.GameObjects;
using GameFramework.GameStructure;
using GameFramework.GameStructure.Levels;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;
using ProPooling;
using UnityEngine.Serialization;
using GameFramework.Weighting;
using GameFramework.Weighting.Components;

namespace GameFramework.Scrolling.Components
{
    /// <summary>
    /// Scrolling script.
    /// </summary>
    [AddComponentMenu("Game Framework/Scrolling/ParallaxLayer")]
    [HelpURL("http://www.flipwebapps.com/game-framework/")]
    public class ParallaxLayer : MonoBehaviour
    {
        #region Public Inspector Values

        //[Tooltip("Whether to set up ready for transitioning in.")]
        //public bool IsLooping;

        [Tooltip("Whether to stop scrolling when Levelmanager.Instance.IsRunning is false.")]
        public bool OnlyScrollWhenLevelRunning;

        [Tooltip("The active area can be limited to the screen for performance, or wider if you want to maintain some history or for other purposes.\n\nItems outside of the active area may be removed.")]
        public bool LimitAreaToScreen = true;

        [ConditionalHide("LimitAreaToScreen", true, true)]
        [Tooltip("The active area can be limited to the screen for performance, or wider if you want to maintain some history or for other purposes.\n\nItems outside of the active area may be removed.")]
        public MinMaxf AreaBounds;

        [Tooltip("The camera to which we are linked.\n\nIf not specified then the main camera is used.")]
        public Camera Camera;

        #region Movement
        [Header("Movement")]

        [Tooltip("Whether we are linked to a camera, or using a fixed scrolling rate.")]
        public bool IsLinkedToCamera;

        [ConditionalHide("IsLinkedToCamera", true, true)]
        [Tooltip("The fixed scrolling speed that should be used.")]
        public float FixedSpeed = 5;

        [ConditionalHide("IsLinkedToCamera", true, true)]
        [Tooltip("Whether to scroll this gameobject, or it's children.\n\nWhich option to chose depends on how else you use this gameobject.\n\nNote: If you choose 'This' then you may encounter positioning errors due to floating point precision with long levels!")]
        public ScrollTrargetType ScrollTarget = ScrollTrargetType.This;
        public enum ScrollTrargetType
        {
            This,
            Children
        };

        [ConditionalHide("IsLinkedToCamera", true)]
        [Tooltip("A factor of the current camera movement by which this layer should move.")]
        public Vector2 MovementFactor = new Vector2(1, 0);

        #endregion Movement

        #region Placement
        [Header("Placement")]

        [Tooltip("Minimum and maximum spacing between prefab instances.")]
        public MinMaxf HorizontalSpacing;

        [Tooltip("Whether to use a random vertical position")]
        public bool RandomVerticalPosition;

        [Tooltip("Vertical range for positioning prefab instances.")]
        [ConditionalHide("RandomVerticalPosition", true)]
        public MinMaxf VerticalPosition;

        #endregion Placement

        [Header("Spawning")]

        [Tooltip("Either add child gameobjects with renders, or specify prefabs for use to automatically setup the level.")]
        public SpawnableItem[] SpawnableItems;

        #endregion Public Inspector Values


        // Total distance scrolled from start
        public float DistanceFromStart { get; set; }


        enum MovementDirectionType { Left, Right, None }
        enum LocationType { Left, Right }

        // true if using child renderers, false if using prefabs.
        bool _isManualSetupMode;

        // for calculating movement delta.
        Vector3 _lastCameraPosition;

        // The outer edges of the current display items including spacing (moves with scroll). 
        // If edges are inside AreaBounds then we need to add more items.
        MinMaxf _areaEdges;

        // what is currently active and displayed on screen.
        List<DisplayItemInstance> _activeDisplayItems = new List<DisplayItemInstance>();

        // weights for the different items.
        readonly DistanceWeightedItems<SpawnableItem> _distanceWeightedItems = new DistanceWeightedItems<SpawnableItem>();


        /// <summary>
        /// Initialise and setup the display
        /// </summary>
        void Start()
        {
            // basic validation and setup
            _activeDisplayItems = new List<DisplayItemInstance>();
            if (Camera == null) Camera = Camera.main;
            _lastCameraPosition = Camera.transform.position;

            Assert.AreEqual(true, Camera.orthographic, "This script currently only works with Orthographic cameras!");

            // basic position setup
            if (LimitAreaToScreen)
            {
                AreaBounds.Min = GameManager.Instance.WorldBottomLeftPosition.x;
                AreaBounds.Max = GameManager.Instance.WorldTopRightPosition.x;
            }

            // Setup for manual mode if there are child items manually added.
            _isManualSetupMode = SetupManualMode();

            // If not manual mode then we add our own based upon the passed prefabs.
            Assert.IsFalse(_isManualSetupMode && SpawnableItems.Length != 0, "You cannot add child renderers directly and also specify prefabs to use for" + gameObject.name);
            Assert.IsFalse(!_isManualSetupMode && SpawnableItems.Length == 0, "You must add either child renders or specify prefabs to use for " + gameObject.name);
            if (!_isManualSetupMode)
            {
                // Setup and pre allocate pool of prefabs
                foreach (var displayItem in SpawnableItems)
                {
                    displayItem.Initialise(transform);

                    _distanceWeightedItems.AddItem(displayItem, displayItem.Weights);
                }
                Assert.IsTrue(_distanceWeightedItems.ItemCount == 0 || _distanceWeightedItems.ItemCount == SpawnableItems.Length, 
                    "You must either omit DistanceWeights, or provide for all prefabs. Not doing so will cause unpredictable results!");

                // Complete weighted items setup.
                _distanceWeightedItems.PrepareForUse();

                // setup display
                _areaEdges.Max = _areaEdges.Min = AreaBounds.Min + HorizontalSpacing.RandomValue();
                SetupAutomaticMode();
            }
        }

        #region Update / Movement
        /// <summary>
        /// Scroll items based upon current configuration
        /// </summary>
        void Update()
        {
            if (OnlyScrollWhenLevelRunning && !LevelManager.Instance.IsLevelRunning)
                return;

            // Update positions
            var movementDirection = MovementDirectionType.None;
            if (IsLinkedToCamera)
            {
                if (Camera.transform.position != _lastCameraPosition)
                {
                    var offset = Camera.transform.position - _lastCameraPosition;
                    var factorOffset = new Vector3(offset.x * MovementFactor.x, offset.y * MovementFactor.y, 0);
                    transform.position += factorOffset;
                    movementDirection = factorOffset.x > 0 ? MovementDirectionType.Left : MovementDirectionType.Right;
                    AreaBounds.Min += offset.x;
                    AreaBounds.Max += offset.x;
                    _areaEdges.Min += factorOffset.x;
                    _areaEdges.Max += factorOffset.x;
                    DistanceFromStart += factorOffset.x;

                    _lastCameraPosition = Camera.transform.position;
                }
            }
            else
            {
                float deltaMovement = FixedSpeed*Time.deltaTime;
                movementDirection = deltaMovement > 0 ? MovementDirectionType.Left : MovementDirectionType.Right;
                _areaEdges.Min -= deltaMovement;
                _areaEdges.Max -= deltaMovement;
                DistanceFromStart += deltaMovement;
                if (ScrollTarget == ScrollTrargetType.This)
                {
                    transform.Translate(new Vector3(-deltaMovement, 0, 0));
                }
                else
                {
                    foreach (var displayItem in _activeDisplayItems)
                    {
                        displayItem.Transform.Translate(-deltaMovement, 0, 0);
                    }
                }
            }

            if (movementDirection == MovementDirectionType.Left)
            {
                var firstElement = _activeDisplayItems.First();

                // first check if we can release any pooled prefabs if its bound has gone off the left of the screen .
                if (!_isManualSetupMode && firstElement.GetRightEdge() < AreaBounds.Min)
                {
                    //_areaEdges.Min = firstElement.GetRightEdge();  // TODO do we need to keep spacing between elements here?
                    firstElement.Pool.ReturnToPool(firstElement);
                    //firstElement.Transform.gameObject.SetActive(false);
                    _activeDisplayItems.Remove(firstElement);

                    firstElement = _activeDisplayItems.First();
                    _areaEdges.Min = firstElement.GetLeftEdge();    // TODO do we need to keep spacing between elements here?
                }

                // check if need to add - right edge is inside bounds, and if so then loop / add new.
                if (_areaEdges.Max <= AreaBounds.Max)
                {
                    if (_isManualSetupMode)
                    {
                        // Loop the item that is on the left round to the new position.
                        var space = HorizontalSpacing.RandomValue();
                        _areaEdges.Min = firstElement.GetRightEdge();
                        firstElement.OnReturnToPool();
                        firstElement.OnGetFromPool();
                        firstElement.Transform.position =
                            new Vector3(_areaEdges.Max + firstElement.PaddingLeft + firstElement.GetPivotOffset().x + space,
                                RandomVerticalPosition
                                    ? transform.position.y + VerticalPosition.RandomValue()
                                    : firstElement.Transform.position.y,
                                firstElement.Transform.position.z);
                        _areaEdges.Max += firstElement.GetWidth() + space;

                        // Set the recycled child to the last position of the list.
                        _activeDisplayItems.Remove(firstElement);
                        _activeDisplayItems.Add(firstElement);
                    }
                    else
                    {
                        // Add a new item to the end
                        AddDisplayItemInstance(LocationType.Right);
                    }
                }
            }
            else if (movementDirection == MovementDirectionType.Right)
            {
                var lastElement = _activeDisplayItems.Last();
                // first check if we can release any pooled prefabs if its bound has gone off the right of the screen .
                if (!_isManualSetupMode && lastElement.GetLeftEdge() > AreaBounds.Max)
                {
                    //_areaEdges.Max = lastElement.GetLeftEdge();    // TODO do we need to keep spacing between elements here?
                    lastElement.Pool.ReturnToPool(lastElement);
                    //lastElement.Transform.gameObject.SetActive(false);
                    _activeDisplayItems.Remove(lastElement);

                    lastElement = _activeDisplayItems.Last();
                    _areaEdges.Max = lastElement.GetRightEdge();    // TODO do we need to keep spacing between elements here?
                }

                // check if need to add - left edge is inside bounds, and if so then loop / add new.
                if (_areaEdges.Min >= AreaBounds.Min)
                {
                    if (_isManualSetupMode)
                    {
                        // Loop the item that is on the right round to the new position.
                        var space = HorizontalSpacing.RandomValue();
                        _areaEdges.Max = lastElement.GetLeftEdge() + space;
                        lastElement.OnReturnToPool();
                        lastElement.OnGetFromPool();
                        lastElement.Transform.position =
                            new Vector3(_areaEdges.Min - lastElement.PaddingRight - lastElement.GetWidth() + lastElement.GetPivotOffset().x,
                                RandomVerticalPosition
                                    ? transform.position.y + VerticalPosition.RandomValue()
                                    : lastElement.Transform.position.y,
                                lastElement.Transform.position.z);
                        _areaEdges.Min -= lastElement.GetWidth() + space;

                        // Set the recycled child to the first position of the list.
                        _activeDisplayItems.Remove(lastElement);
                        _activeDisplayItems.Insert(0, lastElement);
                    }
                    else
                    {
                        // Add a new item to the left
                        AddDisplayItemInstance(LocationType.Left);
                    }
                }
            }
        }
        #endregion Update / Movement


        /// <summary>
        /// Draw Gizmos to show the current area bounds and edges.
        /// </summary>
        void OnDrawGizmos()
        {
            if (!GameManager.IsActive) return;
            var screenHeight = GameManager.Instance.WorldTopRightPosition.y -GameManager.Instance.WorldBottomLeftPosition.y;
            MyDebug.DrawGizmoRect(new Rect(AreaBounds.Min, GameManager.Instance.WorldBottomLeftPosition.y, AreaBounds.Max - AreaBounds.Min, screenHeight), Color.red);
            MyDebug.DrawGizmoRect(new Rect(_areaEdges.Min, GameManager.Instance.WorldBottomLeftPosition.y, _areaEdges.Max - _areaEdges.Min, screenHeight), Color.green);

            foreach (var displayItem in _activeDisplayItems)
            {
                MyDebug.DrawGizmoRect(new Rect(displayItem.GetLeftEdge(), GameManager.Instance.WorldBottomLeftPosition.y + .2f, displayItem.GetWidth(), screenHeight - .4f), Color.blue);
                MyDebug.DrawGizmoRect(new Rect(displayItem.GetLeftEdge() + displayItem.PaddingLeft, GameManager.Instance.WorldBottomLeftPosition.y + .3f, displayItem.GetWidth() - displayItem.PaddingLeft - displayItem.PaddingRight, screenHeight - .6f), Color.cyan);
            }

        }


        /// <summary>
        /// Check and use any child gameobjects that have a renderer component for manual setup mode.
        /// </summary>
        /// <returns></returns>
        bool SetupManualMode()
        {
            var isManualSetupMode = false;

            for (var i = 0; i < transform.childCount; i++)
            {
                var childTransform = transform.GetChild(i);
                //var displayItemInstance = DisplayItemInstance.TryCreate(false, childTransform);
                var displayItemInstance = new DisplayItemInstance() { GameObject = childTransform.gameObject };
                if (displayItemInstance.Renderer != null || displayItemInstance.ParallaxItem != null)
                {
                    _activeDisplayItems.Add(displayItemInstance);
                    isManualSetupMode = true;
                }
            }

            // Sort list by display position left to right and update bounds.
            if (isManualSetupMode)
            {
                _activeDisplayItems = _activeDisplayItems.OrderBy(t => t.Transform.position.x).ToList();
                _areaEdges.Min = _activeDisplayItems.First().GetLeftEdge() - HorizontalSpacing.RandomValue();
                _areaEdges.Max = _activeDisplayItems.Last().GetRightEdge() + HorizontalSpacing.RandomValue();
            }

            return isManualSetupMode;
        }


        /// <summary>
        /// Automatically fill display with prefabs from left of bounds to the right of bounds
        /// </summary>
        void SetupAutomaticMode()
        {
            do
            {
                AddDisplayItemInstance(LocationType.Right);
            } while (_areaEdges.Max <= AreaBounds.Max);
        }


        /// <summary>
        /// Add a (pooled) prefab instance at the specified location.
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        void AddDisplayItemInstance(LocationType location)
        {
            // display item either random, or from weighting.
            var spawnableItem = _distanceWeightedItems.ItemCount == 0 ? 
                SpawnableItems[Random.Range(0, SpawnableItems.Length)] : 
                _distanceWeightedItems.GetItemForDistance((int)DistanceFromStart);

            // get and setup pooled instance. position should be prefab relative to current position.
            var loopingElement = spawnableItem.GetPoolItemFromPool(transform);//.position + spawnableItem.Prefab.transform.position, 
                //transform.rotation * spawnableItem.Prefab.transform.rotation, transform);
            loopingElement.Pool = spawnableItem;

            float xPosition;
            if (location == LocationType.Right)
            {
                xPosition = _areaEdges.Max;
                _areaEdges.Max += loopingElement.GetWidth() + HorizontalSpacing.RandomValue();
                _activeDisplayItems.Add(loopingElement);
            }
            else
            {
                xPosition = _areaEdges.Min - loopingElement.GetWidth();
                _areaEdges.Min -= loopingElement.GetWidth() + HorizontalSpacing.RandomValue();
                _activeDisplayItems.Insert(0, loopingElement);
            }
            loopingElement.Transform.position =
                new Vector3(xPosition + loopingElement.GetPivotOffset().x + loopingElement.PaddingLeft,
                            loopingElement.Transform.position.y + (RandomVerticalPosition ? VerticalPosition.RandomValue() : 0),
                            loopingElement.Transform.position.z);
        }



        [Serializable]
        public class SpawnableItem : PoolGeneric<DisplayItemInstance>
        {
            public List<DistanceWeightValue> Weights { get; set; }

            public SpawnableItem(GameObject prefab, int preInitialiseCount = 0, int initialCapacity = 5, int maxCapacity = 0, Transform inactiveParent = null) :
            base(prefab, preInitialiseCount, initialCapacity, maxCapacity, inactiveParent)
            {
            }
            //Transform _parent;

            // parent (used for holding instances)
            public void Initialise(Transform parent)
            {
                //_parent = parent;
                //base.InactiveParent = _parent;
                base.Initialise();

                //// setup pooled instances
                //_instances = new DisplayItemInstance[PoolCount];
                //for (var i = 0; i < PoolCount; i++)
                //{
                //    CreatePooledInstance(i);
                //}

                // setup weights.
                var distanceWeight = Prefab.GetComponent<DistanceWeight>();
                if (distanceWeight != null)
                {
                    Weights = new List<DistanceWeightValue>(distanceWeight.Weights);
                }
            }

            //// TODO replace pooling with generic pool components
            //// get available Instance
            //public DisplayItemInstance GetInstanceFromPool()
            //{
            //    DisplayItemInstance pooledInstance = null;
            //    foreach (var displayItemInstance in _instances)
            //        if (displayItemInstance.Transform.gameObject.activeSelf == false)
            //        {
            //            pooledInstance = displayItemInstance;
            //            break;
            //            ;
            //        }

            //    if (pooledInstance == null)
            //        pooledInstance = CreatePooledInstance();
            //    pooledInstance.Transform.localPosition = Prefab.localPosition;
            //    pooledInstance.PooledReuse();
            //    pooledInstance.Transform.gameObject.SetActive(true);
            //    foreach(var poolComponent in pooledInstance.IPoolComponents)
            //        poolComponent.OnGetFromPool();

            //    return pooledInstance;
            //}


            //// TODO replace pooling with generic pool components
            ///// <summary>
            ///// Create a pooled prefab instance by adding an additional position.
            ///// </summary>
            ///// <param name="displayItem"></param>
            ///// <returns></returns>
            //protected DisplayItemInstance CreatePooledInstance()
            //{
            //    Array.Resize<DisplayItemInstance>(ref _instances, _instances.Length + 1);
            //    return CreatePooledInstance(_instances.Length - 1);
            //}


            //// TODO replace pooling with generic pool components
            ///// <summary>
            ///// Create a pooled prefab instance at the given index.
            ///// </summary>
            ///// <param name="displayItem"></param>
            ///// <param name="index"></param>
            ///// <returns></returns>
            //DisplayItemInstance CreatePooledInstance(int index)
            //{
            //    var newTransform = (Transform)Instantiate(Prefab);
            //    newTransform.SetParent(_parent, false);                       // prefab is now offset from the parent by its specified position.
            //    newTransform.gameObject.SetActive(false);
            //    var displayItemInstance = DisplayItemInstance.TryCreate(true, newTransform);
            //    Assert.IsNotNull(displayItemInstance, "Unable to create a DisplayItemInstance. Check the prefab " + Prefab.name + " contains either a renderer or a ParallexItem component.");
            //    _instances[index] = displayItemInstance;

            //    return displayItemInstance;
            //}
        }


        /// <summary>
        /// The Unity editor doesn't let us expose generic types other than List<>. Therefor we have this dummy class
        /// to allow us to expose Pool<ParallaxPoolItem> as a non generic class.
        /// </summary>
        [Serializable]
        public class ParallexPoolEditor : PoolGeneric<DisplayItemInstance>
        {
            public ParallexPoolEditor(GameObject prefab, int preInitialiseCount = 0, int initialCapacity = 5, int maxCapacity = 0, Transform inactiveParent = null) :
            base(prefab, preInitialiseCount, initialCapacity, maxCapacity, inactiveParent)
            {
            }
        }

        public class DisplayItemInstance : PoolItem
        {
            public Transform Transform;
            public Vector3 OriginalPosition;
            public Renderer Renderer;
            public ParallaxItem ParallaxItem;
            //public bool AutoCreated;
            public float PaddingLeft;
            public float PaddingRight;
            bool _sizeFromRenderer;

            public override void OnSetup()
            {
                base.OnSetup();
                Transform = GameObject.transform;
                Renderer = GameObject.GetComponent<Renderer>();
                ParallaxItem = GameObject.GetComponent<ParallaxItem>();
                _sizeFromRenderer = Renderer != null && (ParallaxItem == null || ParallaxItem.SizeFromRenderer);

            }

            public override void OnGetFromPool()
            {
                base.OnGetFromPool();

                if (ParallaxItem != null)
                {
                    PaddingLeft = ParallaxItem.GetNewPaddingLeft();
                    PaddingRight = ParallaxItem.GetNewPaddingRight();
                }
            }

            public override void OnReturnToPool()
            {
                base.OnReturnToPool();
            }

            public override void OnDestroy()
            {
                base.OnDestroy();
            }


            // get the render pivot offset (in world space). Used when setting the position.
            public Vector3 GetPivotOffset()
            {
                if (_sizeFromRenderer)
                    return Renderer.transform.position - Renderer.bounds.min;
                else
                    return Vector3.zero;
            }

            // get the width including any padding
            public float GetWidth()
            {
                if (_sizeFromRenderer)
                    return Renderer.bounds.size.x + PaddingLeft + PaddingRight;
                else
                    return PaddingLeft + PaddingRight;
            }

            // get the left edge including any padding
            public float GetLeftEdge()
            {
                if (_sizeFromRenderer)
                    return Renderer.bounds.min.x - PaddingLeft;
                else
                    return Transform.position.x - PaddingLeft;
            }

            // get the right edge including any padding
            public float GetRightEdge()
            {
                if (_sizeFromRenderer)
                    return Renderer.bounds.max.x + PaddingRight;
                else
                    return Transform.position.x + PaddingRight;
            }
        }


        //public class DisplayItemInstance
        //{
        //    public Transform Transform;
        //    public Vector3 OriginalPosition;
        //    public Renderer Renderer;
        //    public ParallaxItem ParallaxItem;
        //    public bool AutoCreated;
        //    public float PaddingLeft;
        //    public float PaddingRight;
        //    readonly bool _sizeFromRenderer;
        //    public IPoolComponent[] IPoolComponents;

        //    DisplayItemInstance(Transform transform, Renderer renderer, ParallaxItem parallaxItem, bool autoCreated)
        //    {
        //        Transform  = transform;
        //        Renderer = renderer;
        //        ParallaxItem = parallaxItem;
        //        AutoCreated = autoCreated;
        //        IPoolComponents = transform.GetComponentsInChildren<IPoolComponent>();

        //        if (AutoCreated) PooledReuse();
        //        _sizeFromRenderer = Renderer != null && (parallaxItem == null || parallaxItem.SizeFromRenderer);
        //    }

        //    public static DisplayItemInstance TryCreate(bool autoCreated, Transform childTransform)
        //    {
        //        var renderer = childTransform.GetComponent<Renderer>();
        //        var parallaxItem = childTransform.GetComponent<ParallaxItem>();
        //        if (renderer != null || parallaxItem != null)
        //        {
        //            return new DisplayItemInstance(childTransform, renderer, parallaxItem, autoCreated);
        //        }
        //        return null;
        //    }

        //    public void PooledReuse()
        //    {
        //        if (ParallaxItem != null)
        //        {
        //            PaddingLeft = ParallaxItem.GetNewPaddingLeft();
        //            PaddingRight = ParallaxItem.GetNewPaddingRight();
        //        }
        //    }

        //    // get the render pivot offset (in world space). Used when setting the position.
        //    public Vector3 GetPivotOffset()
        //    {
        //        if (_sizeFromRenderer)
        //            return Renderer.transform.position - Renderer.bounds.min;
        //        else
        //            return Vector3.zero;
        //    }

        //    // get the width including any padding
        //    public float GetWidth()
        //    {
        //        if (_sizeFromRenderer)
        //            return Renderer.bounds.size.x + PaddingLeft + PaddingRight;
        //        else
        //            return PaddingLeft + PaddingRight;
        //    }

        //    // get the left edge including any padding
        //    public float GetLeftEdge()
        //    {
        //        if (_sizeFromRenderer)
        //            return Renderer.bounds.min.x - PaddingLeft;
        //        else
        //            return Transform.position.x - PaddingLeft;
        //    }

        //    // get the right edge including any padding
        //    public float GetRightEdge()
        //    {
        //        if (_sizeFromRenderer)
        //            return Renderer.bounds.max.x + PaddingRight;
        //        else
        //            return Transform.position.x + PaddingRight;
        //    }
        //}
    }
}
