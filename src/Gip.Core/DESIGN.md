## GipObject

Many components in the Gip graph inherit from a common base class, `GipObject`. This object provides basic implementation of a number of common features:
+ Parenting
  A Gip pipeline is a nested hierarchy of objects. At the top is `GipPipeline`, there might be children `GipBin`s, and underneath those there might be `GipPad`s under those. Objects can be disconnected from the tree, such as if an element is removed from its bin and added to another element. Or merged, such as when an element is created unattached, has children configured, and then is later parented to another element.
+ Synchronization
  Most operations that are done to the object tree itself are thread synchronized. Manipulation of the tree itself is a distinct process from the potentially asynchronous process being undertaken by the elements themselves, on their separate threads, and thus this restriction should be acceptable. This is accomplished by each element maintaining a syncroot from it's ultimate parent around which a monitor lock is placed. Synchronization of operations on the entire tree keep the programming model simple: at the begininning of every operation and single lock can be obtained, under which most operations can be conducted.
  Merging trees means acquiring a root lock on the destination tree, and the source tree, to ensure no ongoing operations are being conducted on them, after which the parent of the source can be changed, and the root of the children can be updated.
+ Event Routing
  A natural operation in a graph of this kind is for elements to send events to their parents. This will be accomplished in a way similar to the W3C XML event model. The first phase of an event on an element will be to acquire the set of all elements to which the event will be routed. Afterwards, the event will be sent in the 'capturing' phase from the root element down. This gives a chance for parent elements to intercept events sent to children events and modify or replace them. After the event reaches its child, the reverse will be done, firing the event on the parents in upwards order. This gives a chance for parent elements to react to events occuring in child elements.
+ Observable Properties
  A central mechanism for altering property values, including acquiring the locks and sending the events.
+ Observable Collections
  A central mechanism for altering collection values, including acquiring the locks and sending the events.
  
## GipElement : GipObject

The basic type of an element that will participate in a pipeline. A `GipElement` will only be parented with a `GipBin`, and only have `GipPad`s as children.

`GipElement`s are the main building block of Gip. Each element is nested within another element, which is ultimately nested within a bin, which is ultimately nested within a pipeline. 

## GipElementFactory

Each element is created by a `GipElementFactory`. The point of a factory is to document metadata about the elements it can produce, without actually producing that data. A `GipElementFactory` will contain `GipPadTemplate`s that describe the available pads and their capabilities.

## GipPad : GipObject

Pads are the connection points between elements. They come in two varieties: sink and send pads. A `GipSinkPad` can be thought of as a source of data for an element, while `GipSendPad` can be thought of as a destination of data for an element. The job of an element is to present pads to convey its data to the next element. Each pad is configured with a `GipCapList`, which is a collection of `GipCap` elements which describe the allowable formats of data that can either be received by the pad or emitted by the pad. During the linking process a common set of `GipCapList` is formed by negotiating the common caps. An element is then free to push data conforming to the negotiated type to a send pad, or receive data from a sink pad of that expected type.

Two subtypes of `GipPad` will exist: `GipSinkPad` and `GipSendPad`.

## GipPadTemplate

Describes something about a potential future `GipPad` on a `GipElement`. Like pads, these come in two formats: sink and send. They contain the potential capabilities of the pads, and provide a way for the relevent elements to create instances of those pads.

## GipBin : GipElement

A bin is an element that holds other elements. One main purpose of a bin is to manage a collection of elements and ensure they are properly linked. But, the logic for this can be abitrary: it might know what elements will be created, or it might consult a library of available element types to make that determination.

A bin might hide the pads of the elements it contains, exposing its own pads and routing data to the inner elements as it sees fit, or it might serve as a simple proxy to pads of the elements it contains.

## GipPipeline : GipBin

A pipeline is a bin which cannot itself be parented. It serves as the top-level container representing the whole of a pipeline.

