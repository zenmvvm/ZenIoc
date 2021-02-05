# ![Logo](art/icon@64x64.png) ZenIoc
Description here

[![Coverage](https://raw.githubusercontent.com/zenmvvm/ZenIoc/develop/coverage/badge_linecoverage.svg)](https://htmlpreview.github.io/?https://raw.githubusercontent.com/zenmvvm/ZenIoc/develop/coverage/index.html) [![NuGet](https://buildstats.info/nuget/ZenIoc?includePreReleases=false)](https://www.nuget.org/packages/ZenIoc/)![CI](https://github.com/zenmvvm/ZenIoc/workflows/CI/badge.svg?branch=main)


A Template solution directory with:

* .NET Standard 2.0 project, and
  * Best for libraries. Everything uses this API. But no implementation so only good for libraries 
* .NET 5 Unit Test project (implementation that now replaces  .NET Core 3.1 target framework)
  * Have to have an implementation for a unit test runner. So .NET 5 is simple... and fully compatable with .NET Standard



## ToDo 

* Consider publishing minor release on dependabot bump of dependancy if tests pass
* Consider adding back dependabot for GitHub actions, but don't publish release when this happens 



## Steps:

Two workflow options:

*  GUI via GitKraken
* Command line



### Command Line Workflow

```bash
# clone skeleton
git clone https://github.com/z33bs/ZenIoc NewName
cd NewName
# remove commit history
rm -rf .git
# do project renaming stuff
rm rename-project

git init
git add .

# create new branch called main
# will only be able to see when committed
git checkout -b main
git commit -m 'Initial commit'
git branch --list

# create remote and push
curl -u 'z33bs' https://api.github.com/user/repos -d '{"name":"NewName"}'
git remote add origin https://github.com/z33bs/NewName
git push -u origin master

## Goto gitub repo online and create Github Workflow
## This will copy the contents of main.yml to clipboard
## so you can just paste it

# Works on Mac only
cat main.yml | pbcopy 

# Should work everywhere (untested)
apt-get install xclip
xclip -sel c < main.yml
```



### GitKraken Workflow

Download this repo as zip file

Rename root folder name

* Open GitKraken

* Init
  * Name project same
  * Select root folder so that it over-rides / matches the existing folder
  * Don't select gitignore or license as already there
  * Should open with initial commit and bunch of unstaged changes
* STAGE all but .github file workflows/main.yml
  * this can't be pushed / updated to remote due to security concerns
* Amend initial commit

* Push
  * Will ask you to create a remote... do so with your account

***

* Run rename-project.command
  
  * Will rename within files, filenames and directories
  
  * MacOS security gives issues
  
  * ```bash
    sudo chmod 755 rename-project 
    ls -l
    # now should see x for execute
    ./rename-project 
    ```
  
* Review changes in version-history, and ammend intial commit
* Force push to server

***

Ensure local and remote are synced (except for unstageed main.yml file)

Login to github online and Create Github action

* copy content from the local main.yml file into the action
* commit file in git-online
* delete local version
* In GitKraken Pull remote from local



Initialise Gitflow and switch to develop branch



