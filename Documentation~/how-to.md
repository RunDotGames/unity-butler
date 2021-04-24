# Unity Butler: How To

This package uses itch.io's [butler command line](https://itch.io/docs/butler/) to publish webGL builds of your game directly to [itch.io](https://www.itch.io)

* See itch.io's [publishing doc](https://itch.io/docs/butler/pushing.html) for details on the `user/game` fields prompted
* See itch.io's [logging in](https://itch.io/docs/butler/login.html#running-butler-from-ci-builds-travis-ci-gitlab-ci-etc) for more information on the api key prompted

## Install

* From Unity Package Manager, select `Add Package from Git URL`
*  ☝ Enter the clone URL for this repo

## Usage
* On Itch.io:
  * Make a new game
  * Make a new [API Key](https://itch.io/user/settings/api-keys)
* In Unity    
  * Open the Publish Window found under `Window > Unity Butler`
  * If prompted, click the `Install Butler` to download butler into the project asset folder
    * Butler and publish window values will be saved to `Assets\RDG\UnityButler`
    * This folder is automatically ignored from git to avoid distributing unnecessary binary files and confidential API keys.
* Enter all itch.io required fields
* Configure your game's WebGL Player Settings and active scenes as needed
* Click `Publish` from the publish window to build and upload a WebGL Playable version of your game to itch.io

## Limitations

### Windows Only

This package executes Windows `bat` scripts and so is does not support Max/Linux.

### WebGL Only

Butler Publishing assumes WebGL Build channel only.
