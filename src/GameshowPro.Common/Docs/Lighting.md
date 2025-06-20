`Fixtures` is a class within every project using the lighting system. It's a subclass of `FixturesBase`, which manages all the general-purpose logic. Most of the logic in the base class pertains to persistence, depersistence, and merging hard-coded templates with user-modifiable settings.

Each subclass of `FixturesBase` must specify its own hard-coded list of fixtures with all their default properties set appropriately.

`Fixture` represents a single logical lighting unit with a number or channels. In most cases it represents a white pixel, an RGB pixel, or an RGBW pixel.

BuildTemplate builds a default list of Fixture objects. Each of these has:
* `Key`
* `DisplayName`
* `StartId` - The ID number of the first channel in the fixture.
* `Channels` - The list of channels.
* `StateGroup` - The groups of states which are applicable to this fixture. Theoretically, this could be bound to a UI allowing any state to be applied to any group. Whenever `State` is set, it is validated to ensure it belongs to the specified `StateGroup`.
* `State` - The last state to be applied to this fixture. Theoretically, this could be shown on the UI, but currently it is not.

## ID numbers
The ID numbers of each channel in the template are not directly constrained by any system like DMX, ArtNet or sACN. It is up to separate code to map channel IDs to the numbering system used by the outside system. 

The IDs of each channel are not stored individually, but are set every time the `Channels` or `StartId` properties are set one a `Fixture`. `FixChannelIds()` is currently hard-coded to group the ID numbers into universes of 512 channels using `Math.DivRem`. In future, if using non-DMX-contrained universes, this could be modified to offer additional options.

## Preset groups

`StateLevels` represents a group of channel levels which go together to represent a particular state of a fixture. These can generally be edited by a user through the UI, e.g. to represent any fixture which is in the *Buzzed* state. `StateLevels` objects are grouped into a `StatePresetGroup`, which represent all the possible states a particular fixture could be in, e.g. *Buzzed*, *OutOfPlay*, and *Default*.

It's up to the subclass of `FixturesBase` to generate the list of `StatePresetGroup` and assign them to the applicable `Fixture` objects.

## Deserializeation

`Fixtures` instances are created using the `Depersist()` method. This will cause the template to be generated and the depersisted settigns to be applied to it.

## Integration with sACN
After deserialization, a list of `UniverseSettings` objects is available is each instance of `SacnSettings`. The default size of each universe is the maximum (512) but lower powers of 2 may be specified in client code before passing the `SacnSettings` to the `Sacn` constructor. In many cases, it makes sense to calculate highest channel in use and set the universe size to the smallest power of 2 possible.
