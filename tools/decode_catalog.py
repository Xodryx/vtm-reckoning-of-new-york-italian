"""Decode a Unity Addressables content catalog (JSON form) into readable entries."""
import base64
import json
import struct
import sys

CATALOG = sys.argv[1]

with open(CATALOG, encoding="utf-8") as fh:
    cat = json.load(fh)

internal_ids = cat["m_InternalIds"]
provider_ids = cat["m_ProviderIds"]
resource_types = [t["m_ClassName"] for t in cat["m_resourceTypes"]]

key_data = base64.b64decode(cat["m_KeyDataString"])
bucket_data = base64.b64decode(cat["m_BucketDataString"])
entry_data = base64.b64decode(cat["m_EntryDataString"])
extra_data = base64.b64decode(cat["m_ExtraDataString"])


def read_object(buf, offset):
    """Mirror of ObjectInitializationData/SerializationUtilities.ReadObjectFromByteArray."""
    kind = buf[offset]
    offset += 1
    if kind == 0:  # ascii string
        length = struct.unpack_from("<i", buf, offset)[0]
        return buf[offset + 4:offset + 4 + length].decode("ascii", "replace")
    if kind == 1:  # unicode string
        length = struct.unpack_from("<i", buf, offset)[0]
        return buf[offset + 4:offset + 4 + length].decode("utf-16-le", "replace")
    if kind == 2:  # uint32
        return struct.unpack_from("<I", buf, offset)[0]
    if kind == 3:  # uint16
        return struct.unpack_from("<H", buf, offset)[0]
    if kind == 4:  # int32
        return struct.unpack_from("<i", buf, offset)[0]
    if kind == 5:  # hash128 / json object
        length = struct.unpack_from("<i", buf, offset)[0]
        return buf[offset + 4:offset + 4 + length].decode("ascii", "replace")
    return f"<kind {kind}>"


# Buckets: [keyDataOffset, entryCount, entryIndex...]
buckets = []
pos = 0
while pos < len(bucket_data):
    key_offset, count = struct.unpack_from("<ii", bucket_data, pos)
    pos += 8
    entries = list(struct.unpack_from(f"<{count}i", bucket_data, pos))
    pos += 4 * count
    buckets.append((key_offset, entries))

keys = [read_object(key_data, off) for off, _ in buckets]

# Entries: 7 int32 each
ENTRY_SIZE = 7 * 4
entry_count = struct.unpack_from("<i", entry_data, 0)[0]
entries = []
for i in range(entry_count):
    base = 4 + i * ENTRY_SIZE
    (internal_id, provider, dep_key, dep_hash,
     data_index, primary_key, type_index) = struct.unpack_from("<7i", entry_data, base)
    entries.append({
        "internalId": internal_ids[internal_id],
        "provider": provider_ids[provider],
        "depKey": dep_key,
        "primaryKey": primary_key,
        "type": resource_types[type_index] if type_index >= 0 else None,
    })

print(f"entries: {entry_count}   keys: {len(keys)}   internalIds: {len(internal_ids)}")
print("\n=== resource types ===")
for t in resource_types:
    print("  ", t)

# key index -> entry indices, so we can name each entry
entry_to_keys = {}
for ki, (_, ents) in enumerate(buckets):
    for e in ents:
        entry_to_keys.setdefault(e, []).append(keys[ki])

print("\n=== entries whose type mentions Localization/Language ===")
for i, e in enumerate(entries):
    if e["type"] and ("Loc" in e["type"] or "Language" in e["type"]):
        print(f"[{i}] type={e['type']}")
        print(f"     internalId={e['internalId']}")
        print(f"     keys={entry_to_keys.get(i)}")
        print(f"     provider={e['provider']}")

if "--dump-keys" in sys.argv:
    print("\n=== all keys ===")
    for k in keys:
        print("  ", k)
