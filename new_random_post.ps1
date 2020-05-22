# generate hugo's post with random name

$name = [guid]::NewGuid()

hugo.exe new "posts/$($name).md"