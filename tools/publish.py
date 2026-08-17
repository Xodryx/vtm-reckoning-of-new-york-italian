#!/usr/bin/env python3
"""Creates the GitHub release and attaches the packages.

Called by release.sh, which owns the version and the tag; this only talks to the API.
Both operations are written to be repeatable: publishing the same version twice updates
the release and replaces the attachments rather than failing or, worse, leaving a
release with the old files still on it.

    python tools/publish.py release <repo> <version> <notes.md> <token>
    python tools/publish.py asset   <repo> <release_id> <file> <token>

The token comes from git's credential helper and is passed as an argument rather than
read here, so this file never touches the store.
"""

import io
import json
import os
import sys
import urllib.error
import urllib.request

API = "https://api.github.com"
UPLOADS = "https://uploads.github.com"


def call(url, token, method="GET", body=None, content_type="application/json"):
    request = urllib.request.Request(url, data=body, method=method)
    request.add_header("Authorization", f"token {token}")
    request.add_header("Accept", "application/vnd.github+json")
    if body is not None:
        request.add_header("Content-Type", content_type)
    try:
        with urllib.request.urlopen(request) as response:
            payload = response.read()
            return response.status, (json.loads(payload) if payload else {})
    except urllib.error.HTTPError as error:
        payload = error.read()
        try:
            return error.code, json.loads(payload)
        except ValueError:
            return error.code, {"message": payload.decode("utf-8", "replace")}


def release(repo, version, notes_path, token):
    tag = f"v{version}"
    notes = io.open(notes_path, encoding="utf-8").read().replace("@VERSION@", version)
    fields = {
        "tag_name": tag,
        "name": f"{version} — traduzione italiana",
        "body": notes,
        "draft": False,
        # Below 1.0 the translation has not been read back by a person, and GitHub's own
        # label is the honest place to say so.
        "prerelease": version.startswith("0."),
    }
    body = json.dumps(fields, ensure_ascii=False).encode("utf-8")

    status, existing = call(f"{API}/repos/{repo}/releases/tags/{tag}", token)
    if status == 200:
        status, data = call(f"{API}/repos/{repo}/releases/{existing['id']}",
                            token, "PATCH", body)
    else:
        status, data = call(f"{API}/repos/{repo}/releases", token, "POST", body)

    if status not in (200, 201):
        sys.exit(f"la release non è stata creata: http {status} {data.get('message')}")
    return data["id"]


def asset(repo, release_id, path, token):
    name = os.path.basename(path)

    # An asset of the same name cannot be overwritten, and leaving the old one behind
    # would publish a package that does not match its own release notes.
    status, data = call(f"{API}/repos/{repo}/releases/{release_id}/assets", token)
    if status == 200:
        for existing in data:
            if existing["name"] == name:
                call(f"{API}/repos/{repo}/releases/assets/{existing['id']}",
                     token, "DELETE")

    with io.open(path, "rb") as handle:
        payload = handle.read()

    status, data = call(f"{UPLOADS}/repos/{repo}/releases/{release_id}/assets?name={name}",
                        token, "POST", payload, "application/zip")
    if status not in (200, 201):
        sys.exit(f"caricamento fallito: http {status} {data.get('message')}")
    return data["size"]


def main():
    if len(sys.argv) < 5:
        sys.exit(__doc__)

    action = sys.argv[1]
    if action == "release":
        print(release(sys.argv[2], sys.argv[3], sys.argv[4], sys.argv[5]))
    elif action == "asset":
        size = asset(sys.argv[2], sys.argv[3], sys.argv[4], sys.argv[5])
        print(f"caricato, {size / 1e6:.1f} MB")
    else:
        sys.exit(f"azione sconosciuta: {action}")


if __name__ == "__main__":
    main()
