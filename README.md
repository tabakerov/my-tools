`guidtoclipboard` generates new guid and puts it to system clipboard

`new_random_post` generates hugo's post using random guid as a name

`process_and_uplad_images.fsx` renames with random name and resizes given image (or images if a folder is provided) to a set of images with 3840, 1920, 720 and 480 pixels width and then uploads this images to Azure Storage (connection string should be in the environment or `config` file). Usage:
- `dotnet fsi process_and_uplad_images.fsx path/to/image/or/dir/with/images` (process given image or all files in the folder)
- `dotnet fsi process_and_uplad_images.fsx path/to/dir/with/images png jpg` (to process images in the folder filtered by the given list of extensions
