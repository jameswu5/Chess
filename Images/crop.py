from PIL import Image

def crop_image(image_path, top_left, bottom_right, output_path):
    # Open the image file
    with Image.open(image_path) as img:
        # Calculate the cropping box
        left, top = top_left
        right, bottom = bottom_right
        crop_box = (left, top, right, bottom)
        
        # Crop the image
        cropped_image = img.crop(crop_box)
        
        # Save the cropped image
        cropped_image.save(output_path)


piece_x = {"King": 0, "Queen": 333, "Bishop": 666, "Knight": 1000, "Rook": 1333, "Pawn": 1666}
piece_y = {"White": 0, "Black": 332}

height = 332
width = 333

def crop_pieces(piece, colour):
    top_left = (piece_x[piece], piece_y[colour])
    bottom_right = (piece_x[piece] + width, piece_y[colour] + height)
    crop_image('Images/Pieces.png', top_left, bottom_right, f'Images/{colour}{piece}.png')

def main():
    for piece in piece_x.keys():
        for colour in piece_y.keys():
            crop_pieces(piece, colour)

if __name__ == '__main__':
    main()