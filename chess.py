#########################################
# CHESS                                 #
#                                       #
# Created: June 2020                    #
# Author: James Wu                      #
#                                       #
# Images snipped from chess software    #
#                                       #
#########################################
#                                       #
# Requirements:                         #
# Python 3.8.3, Pygame 1.9.6            #
#                                       #
#########################################

# Features:
# All standard rules
# User friendly interface
# Highlighted squares to show where you can go
# Castling
# Pawn promotion (auto-promote to queen)
# If King gets captured, you lose.

# Missing features:
# en passant
# checks
# checkmate

import pygame
import sys

# Constants
SCREEN_WIDTH = 760
SCREEN_HEIGHT = 740
# How much time do you want in minutes? For chess clock
CHESS_TIME = 5
FRAMERATE = 40

# Constants for colours, so that it can be changed easily
WHITE = (255, 248, 245)
TEXT_COLOUR = (0, 0, 0)
BACKGROUND_COLOUR = (192, 192, 192)
BLUE = (32, 178, 178)
BROWN = (160, 82, 45)
GREEN = (152, 251, 152)
DARK = (122, 231, 122) # A darker shade of Green
YELLOW = (240, 230, 140)
BLACK = (0, 0, 0)

# Loads all images of pieces
wRook = pygame.image.load("wRook.png")
wPawn = pygame.image.load("wPawn.png")
wKnight = pygame.image.load("wKnight.png")
wBishop = pygame.image.load("wBishop.png")
wQueen = pygame.image.load("wQueen.png")
wKing = pygame.image.load("wKing.png")

bRook = pygame.image.load("bRook.png")
bPawn = pygame.image.load("bPawn.png")
bKnight = pygame.image.load("bKnight.png")
bBishop = pygame.image.load("bBishop.png")
bQueen = pygame.image.load("bQueen.png")
bKing = pygame.image.load("bKing.png")

# Resizes the images to fit the board
def resize(image):
    return pygame.transform.scale(image, (80, 80))

wRook = resize(wRook)
wPawn = resize(wPawn)
wKnight = resize(wKnight)
wBishop = resize(wBishop)
wQueen = resize(wQueen)
wKing = resize(wKing)

bRook = resize(bRook)
bPawn = resize(bPawn)
bKnight = resize(bKnight)
bBishop = resize(bBishop)
bQueen = resize(bQueen)
bKing = resize(bKing)

# Initialisation
pygame.init()
pygame.font.init()

# For the move sound effect
pygame.mixer.init()
makemove = pygame.mixer.Sound("Move.wav")

screen = pygame.display.set_mode((SCREEN_WIDTH, SCREEN_HEIGHT))
pygame.display.set_caption("Chess")
clock = pygame.time.Clock()

# Generates the chessboard
chessboard = []
board = open("chessboard.txt", "r")
for h in board.readlines():
    row = list(h.strip())
    chessboard.append(row)
# Generates the model storing position of pieces
pieces = open("chess_pieces.txt", "r")
chess_pieces = []
for h in pieces.readlines():
    row = list(h.strip())
    chess_pieces.append(row)

# Useful for deciding if a piece or mouseclick is out of range
positions = []
for i in range(8):
    for j in range(8):
        positions.append([i,j])

# Displays the chessboard based on the model
def display_board():
    screen.fill(BACKGROUND_COLOUR)
    for i in range(8):
        for j in range(8):
            if chessboard[i][j] == "W":
                pygame.draw.rect(screen, WHITE, pygame.Rect(i*(80), j*(80), 80, 80))
            elif chessboard[i][j] == "B":
                pygame.draw.rect(screen, BLUE, pygame.Rect(i*(80), j*(80), 80, 80))
            elif chessboard[i][j] == "G":
                pygame.draw.rect(screen, GREEN, pygame.Rect(i*(80), j*(80), 80, 80))
            elif chessboard[i][j] == "D":
                pygame.draw.rect(screen, DARK, pygame.Rect(i*(80), j*(80), 80, 80))
            elif chessboard[i][j] == "Y":
                pygame.draw.rect(screen, YELLOW, pygame.Rect(i*(80), j*(80), 80, 80))


# Displays and draws the pieces on the board based on the model
def display_pieces():
    for i in range(8):
        for j in range(8):
            if chess_pieces[i][j] == "R":
                screen.blit(wRook, pygame.Rect(i*80, j*80, 80, 80))
            elif chess_pieces[i][j] == "N":
                screen.blit(wKnight, pygame.Rect(i*80, j*80, 80, 80))
            elif chess_pieces[i][j] == "B":
                screen.blit(wBishop, pygame.Rect(i*80, j*80, 80, 80))
            elif chess_pieces[i][j] == "Q":
                screen.blit(wQueen, pygame.Rect(i*80, j*80, 80, 80))
            elif chess_pieces[i][j] == "K":
                screen.blit(wKing, pygame.Rect(i*80, j*80, 80, 80))
            elif chess_pieces[i][j] == "P":
                screen.blit(wPawn, pygame.Rect(i*80, j*80, 80, 80))

            elif chess_pieces[i][j] == "r":
                screen.blit(bRook, pygame.Rect(i*80, j*80, 80, 80))
            elif chess_pieces[i][j] == "n":
                screen.blit(bKnight, pygame.Rect(i*80, j*80, 80, 80))
            elif chess_pieces[i][j] == "b":
                screen.blit(bBishop, pygame.Rect(i*80, j*80, 80, 80))
            elif chess_pieces[i][j] == "q":
                screen.blit(bQueen, pygame.Rect(i*80, j*80, 80, 80))
            elif chess_pieces[i][j] == "k":
                screen.blit(bKing, pygame.Rect(i*80, j*80, 80, 80))
            elif chess_pieces[i][j] == "p":
                screen.blit(bPawn, pygame.Rect(i*80, j*80, 80, 80))

# Returns a coordinate for the chessboard based on mouse position
def get_mouse_position():
    position = pygame.mouse.get_pos()
    x, y = position[0], position[1]
    return x//80, y//80

# This subroutine does the actual moving
def moving(x, y, newx, newy):
    position = []
    newposition = []
    position.append(x)
    position.append(y)
    newposition.append(newx)
    newposition.append(newy)
    if position != newposition:
        current = chess_pieces[x][y]
        chess_pieces[newx][newy] = current
        chess_pieces[x][y] = '.'

# Checks if there is a piece
def there_piece(x,y):
    if chess_pieces[x][y] != '.':
        return True
    return False

# Switches the player's turn
def switch_turn(turn):
    if turn == "WHITE":
        newturn = "BLACK"
    elif turn == "BLACK":
        newturn = "WHITE"
    return newturn

# Finds the piece colour of the selected square
def find_colour(x, y):
    if chess_pieces[x][y].isupper():
        return "WHITE"
    elif chess_pieces[x][y].islower():
        return "BLACK"

# Displays whose turn it is
def display_turn():
    pygame.font.init()
    myfont = pygame.font.SysFont("Calibri", 40)
    display = myfont.render(turn.lower() + " to move", False, TEXT_COLOUR)
    screen.blit(display, (200, 670))

# Displays the chess clock
def display_time():
    pygame.font.init()
    # time left is in seconds
    timeLeft_White = CHESS_TIME * 60 - timeSpentWhite // FRAMERATE
    if timeLeft_White == 0:
        return 'Black'
    white_minutes = str(timeLeft_White // 60)
    white_seconds = str(timeLeft_White % 60)
    if len(white_seconds) == 1:
        white_seconds = '0' + white_seconds
    timeLeft_Black = CHESS_TIME * 60 - timeSpentBlack // FRAMERATE
    if timeLeft_Black == 0:
        return 'White'
    black_minutes = str(timeLeft_Black // 60)
    black_seconds = str(timeLeft_Black % 60)
    if len(black_seconds) == 1:
        black_seconds = '0' + black_seconds

    timefont = pygame.font.SysFont("Calibri", 40)
    whiteTime = timefont.render(white_minutes + ':' + white_seconds, False, TEXT_COLOUR)
    blackTime = timefont.render(black_minutes + ':' + black_seconds, False, TEXT_COLOUR)
    screen.blit(whiteTime, (660, 580))
    screen.blit(blackTime, (660, 40))


# This class checks for possible moves
# And sets the rules for every piece
class Move:
    def __init__(self, posx, posy):
        self.x = posx
        self.y = posy

    def Knight(self):
        # List storing all possible moves, not considering illegal moves
        possiblemoves = [
            [self.x + 2, self.y + 1],
            [self.x + 2, self.y - 1],
            [self.x - 2, self.y + 1],
            [self.x - 2, self.y - 1],
            [self.x + 1, self.y + 2],
            [self.x + 1, self.y - 2],
            [self.x - 1, self.y + 2],
            [self.x - 1, self.y - 2],
        ]

        cantmoves = []      # Finds illegal moves
        for move in possiblemoves:
            if move not in positions:
                cantmoves.append(move)
            else:
                if find_colour(move[0], move[1]) == find_colour(x, y):
                    cantmoves.append(move)
        # Removes all illegal moves from possiblemoves
        for nomove in cantmoves:
            possiblemoves.remove(nomove)
        
        # Highlights all squares where the knight can move to
        for possiblemove in possiblemoves:
            if chessboard[possiblemove[0]][possiblemove[1]] ==  'W':
                chessboard[possiblemove[0]][possiblemove[1]] =  'G'
            else:
                chessboard[possiblemove[0]][possiblemove[1]] =  'D'

        return possiblemoves

    def King(self):
        possiblemoves = [
            [self.x + 1, self.y + 1],
            [self.x + 1, self.y],
            [self.x + 1, self.y - 1],
            [self.x, self.y - 1],
            [self.x, self.y + 1],
            [self.x - 1, self.y + 1],
            [self.x - 1, self.y],
            [self.x - 1, self.y - 1]
        ]

        cantmoves = []
        for move in possiblemoves:
            if move not in positions:
                cantmoves.append(move)
            else:
                if find_colour(move[0], move[1]) == find_colour(x, y):
                    cantmoves.append(move)
        for nomove in cantmoves:
            possiblemoves.remove(nomove)

        # Castling kingside
        if find_colour(x, y) == "WHITE":
            if x == 4 and y == 7:
                if chess_pieces[5][7] == '.' and chess_pieces[6][7] == '.' and chess_pieces[7][7] == 'R':
                    possiblemoves.append([6, 7])
        else:
            if x == 4 and y == 0:
                if chess_pieces[5][0] == '.' and chess_pieces[6][0] == '.' and chess_pieces[7][0] == 'r':
                    possiblemoves.append([6, 0])
        
        # Castling queenside
        if find_colour(x, y) == "WHITE":
            if x == 4 and y == 7:
                if chess_pieces[3][7] == '.' and chess_pieces[2][7] == '.' and chess_pieces[1][7] == '.' and chess_pieces[0][7] == 'R':
                    possiblemoves.append([2, 7])
        else:
            if x == 4 and y == 0:
                if chess_pieces[3][0] == '.' and chess_pieces[2][0] == '.' and chess_pieces[1][0] == '.' and chess_pieces[0][0] == 'r':
                    possiblemoves.append([2, 0])

        for possiblemove in possiblemoves:
            if chessboard[possiblemove[0]][possiblemove[1]] ==  'W':
                chessboard[possiblemove[0]][possiblemove[1]] =  'G'
            else:
                chessboard[possiblemove[0]][possiblemove[1]] =  'D'

        return possiblemoves

    def whitePawn(self):
        # Starts with an empty list so we can append legal moves later
        possiblemoves = []
        
        if chess_pieces[self.x][self.y-1] == '.':
            possiblemoves.append([self.x, self.y-1])
        
        if self.y == 6:
            if chess_pieces[self.x][self.y-2] == '.':
                possiblemoves.append([self.x, self.y-2])
        
        if [self.x+1, self.y-1] in positions:
            if chess_pieces[self.x+1][self.y-1].islower():
                possiblemoves.append([self.x+1, self.y-1])

        if [self.x-1, self.y-1] in positions:    
            if chess_pieces[self.x-1][self.y-1].islower():
                possiblemoves.append([self.x-1, self.y-1])

        for possiblemove in possiblemoves:
            if chessboard[possiblemove[0]][possiblemove[1]] ==  'W':
                chessboard[possiblemove[0]][possiblemove[1]] =  'G'
            else:
                chessboard[possiblemove[0]][possiblemove[1]] =  'D'

        return possiblemoves

    def blackPawn(self):
        possiblemoves = []
        
        if chess_pieces[self.x][self.y+1] == '.':
            possiblemoves.append([self.x, self.y+1])
        
        if self.y == 1:
            if chess_pieces[self.x][self.y+2] == '.':
                possiblemoves.append([self.x, self.y+2])
        
        if [self.x+1, self.y+1] in positions:
            if chess_pieces[self.x+1][self.y+1].isupper():
                possiblemoves.append([self.x+1, self.y+1])
        
        if [self.x-1, self.y+1] in positions:
            if chess_pieces[self.x-1][self.y+1].isupper():
                possiblemoves.append([self.x-1, self.y+1])

        for possiblemove in possiblemoves:
            if chessboard[possiblemove[0]][possiblemove[1]] ==  'W':
                chessboard[possiblemove[0]][possiblemove[1]] =  'G'
            else:
                chessboard[possiblemove[0]][possiblemove[1]] =  'D'

        return possiblemoves

    def Rook(self):
        possiblemoves = []
        counter = 1
        while True:
            if [self.x, self.y+counter] in positions:
                if chess_pieces[self.x][self.y+counter] == '.':
                    possiblemoves.append([self.x, self.y+counter])
                    counter += 1
                else:
                    if find_colour(self.x, self.y+counter) != find_colour(x, y):
                        possiblemoves.append([self.x, self.y+counter])
                        counter += 1
                    else:
                        break
            else:
                break

        counter = 1
        while True:
            if [self.x, self.y-counter] in positions:
                if chess_pieces[self.x][self.y-counter] == '.':
                    possiblemoves.append([self.x, self.y-counter])
                    counter += 1
                else:
                    if find_colour(self.x, self.y-counter) != find_colour(x, y):
                        possiblemoves.append([self.x, self.y-counter])
                        counter += 1
                        break
                    else:
                        break
            else:
                break
        
        counter = 1
        while True:
            if [self.x + counter, self.y] in positions:
                if chess_pieces[self.x + counter][self.y] == '.':
                    possiblemoves.append([self.x + counter, self.y])
                    counter += 1
                else:
                    if find_colour(self.x + counter, self.y) != find_colour(x, y):
                        possiblemoves.append([self.x + counter, self.y])
                        counter += 1
                        break
                    else:
                        break
            else:
                break

        counter = 1
        while True:
            if [self.x - counter, self.y] in positions:
                if chess_pieces[self.x - counter][self.y] == '.':
                    possiblemoves.append([self.x - counter, self.y])
                    counter += 1
                else:
                    if find_colour(self.x - counter, self.y) != find_colour(x, y):
                        possiblemoves.append([self.x - counter, self.y])
                        counter += 1
                        break
                    else:
                        break
            else:
                break

        for possiblemove in possiblemoves:
            if chessboard[possiblemove[0]][possiblemove[1]] ==  'W':
                chessboard[possiblemove[0]][possiblemove[1]] =  'G'
            else:
                chessboard[possiblemove[0]][possiblemove[1]] =  'D'

        return possiblemoves


    def Bishop(self):
        possiblemoves = []
        counter = 1
        while True:
            if [self.x+counter, self.y+counter] in positions:
                if chess_pieces[self.x+counter][self.y+counter] == '.':
                    possiblemoves.append([self.x+counter, self.y+counter])
                    counter += 1
                else:
                    if find_colour(self.x+counter, self.y+counter) != find_colour(x, y):
                        possiblemoves.append([self.x+counter, self.y+counter])
                        counter += 1
                        break
                    else:
                        break
            else:
                break
        
        counter = 1
        while True:
            if [self.x-counter, self.y+counter] in positions:
                if chess_pieces[self.x-counter][self.y+counter] == '.':
                    possiblemoves.append([self.x-counter, self.y+counter])
                    counter += 1
                else:
                    if find_colour(self.x-counter, self.y+counter) != find_colour(x, y):
                        possiblemoves.append([self.x-counter, self.y+counter])
                        counter += 1
                        break
                    else:
                        break
            else:
                break
        
        counter = 1
        while True:
            if [self.x+counter, self.y-counter] in positions:
                if chess_pieces[self.x+counter][self.y-counter] == '.':
                    possiblemoves.append([self.x+counter, self.y-counter])
                    counter += 1
                else:
                    if find_colour(self.x+counter, self.y-counter) != find_colour(x, y):
                        possiblemoves.append([self.x+counter, self.y-counter])
                        counter += 1
                        break
                    else:
                        break
            else:
                break
            
        counter = 1
        while True:
            if [self.x-counter, self.y-counter] in positions:
                if chess_pieces[self.x-counter][self.y-counter] == '.':
                    possiblemoves.append([self.x-counter, self.y-counter])
                    counter += 1
                else:
                    if find_colour(self.x-counter, self.y-counter) != find_colour(x, y):
                        possiblemoves.append([self.x-counter, self.y-counter])
                        counter += 1
                        break
                    else:
                        break
            else:
                break

        for possiblemove in possiblemoves:
            if chessboard[possiblemove[0]][possiblemove[1]] ==  'W':
                chessboard[possiblemove[0]][possiblemove[1]] =  'G'
            else:
                chessboard[possiblemove[0]][possiblemove[1]] =  'D'

        return possiblemoves

    def Queen(self):
        possiblemoves = []
        counter = 1
        while True:
            if [self.x+counter, self.y+counter] in positions:
                if chess_pieces[self.x+counter][self.y+counter] == '.':
                    possiblemoves.append([self.x+counter, self.y+counter])
                    counter += 1
                else:
                    if find_colour(self.x+counter, self.y+counter) != find_colour(x, y):
                        possiblemoves.append([self.x+counter, self.y+counter])
                        counter += 1
                        break
                    else:
                        break
            else:
                break
        
        counter = 1
        while True:
            if [self.x-counter, self.y+counter] in positions:
                if chess_pieces[self.x-counter][self.y+counter] == '.':
                    possiblemoves.append([self.x-counter, self.y+counter])
                    counter += 1
                else:
                    if find_colour(self.x-counter, self.y+counter) != find_colour(x, y):
                        possiblemoves.append([self.x-counter, self.y+counter])
                        counter += 1
                        break
                    else:
                        break
            else:
                break
        
        counter = 1
        while True:
            if [self.x+counter, self.y-counter] in positions:
                if chess_pieces[self.x+counter][self.y-counter] == '.':
                    possiblemoves.append([self.x+counter, self.y-counter])
                    counter += 1
                else:
                    if find_colour(self.x+counter, self.y-counter) != find_colour(x, y):
                        possiblemoves.append([self.x+counter, self.y-counter])
                        counter += 1
                        break
                    else:
                        break
            else:
                break
            
        counter = 1
        while True:
            if [self.x-counter, self.y-counter] in positions:
                if chess_pieces[self.x-counter][self.y-counter] == '.':
                    possiblemoves.append([self.x-counter, self.y-counter])
                    counter += 1
                else:
                    if find_colour(self.x-counter, self.y-counter) != find_colour(x, y):
                        possiblemoves.append([self.x-counter, self.y-counter])
                        counter += 1
                        break
                    else:
                        break
            else:
                break

        counter = 1
        while True:
            if [self.x, self.y+counter] in positions:
                if chess_pieces[self.x][self.y+counter] == '.':
                    possiblemoves.append([self.x, self.y+counter])
                    counter += 1
                else:
                    if find_colour(self.x, self.y+counter) != find_colour(x, y):
                        possiblemoves.append([self.x, self.y+counter])
                        counter += 1
                    else:
                        break
            else:
                break

        counter = 1
        while True:
            if [self.x, self.y-counter] in positions:
                if chess_pieces[self.x][self.y-counter] == '.':
                    possiblemoves.append([self.x, self.y-counter])
                    counter += 1
                else:
                    if find_colour(self.x, self.y-counter) != find_colour(x, y):
                        possiblemoves.append([self.x, self.y-counter])
                        counter += 1
                        break
                    else:
                        break
            else:
                break
        
        counter = 1
        while True:
            if [self.x + counter, self.y] in positions:
                if chess_pieces[self.x + counter][self.y] == '.':
                    possiblemoves.append([self.x + counter, self.y])
                    counter += 1
                else:
                    if find_colour(self.x + counter, self.y) != find_colour(x, y):
                        possiblemoves.append([self.x + counter, self.y])
                        counter += 1
                        break
                    else:
                        break
            else:
                break

        counter = 1
        while True:
            if [self.x - counter, self.y] in positions:
                if chess_pieces[self.x - counter][self.y] == '.':
                    possiblemoves.append([self.x - counter, self.y])
                    counter += 1
                else:
                    if find_colour(self.x - counter, self.y) != find_colour(x, y):
                        possiblemoves.append([self.x - counter, self.y])
                        counter += 1
                        break
                    else:
                        break
            else:
                break

        for possiblemove in possiblemoves:
            if chessboard[possiblemove[0]][possiblemove[1]] ==  'W':
                chessboard[possiblemove[0]][possiblemove[1]] =  'G'
            else:
                chessboard[possiblemove[0]][possiblemove[1]] =  'D'

        return possiblemoves


select = False
turn = "WHITE"

# Variable used to decide whether the game is won or lost
whiteKingPresent = True
blackKingPresent = True

# Variable used to store time, for the in-built chessclock
timeSpentWhite = 0
timeSpentBlack = 0

# Main Game Loop
while True:
    clock.tick(FRAMERATE)

    # For Chess Clock
    if turn == "WHITE":
        timeSpentWhite += 1
    else:
        timeSpentBlack += 1

    # Handling events
    events = pygame.event.get()
    for event in events:
        if event.type == pygame.QUIT:
            pygame.quit()
            sys.exit()
        elif event.type == pygame.KEYDOWN and event.key == pygame.K_ESCAPE:
            pygame.quit()
            sys.exit()

        
        elif event.type == pygame.MOUSEBUTTONDOWN:
            if select == False:
                # Gets an x and y coordinate from 0 to 7, useful for referring to the 2D arrays created earlier
                x, y = get_mouse_position()
                if x < 8 and y < 8:
                    if there_piece(x, y):
                        piece = chess_pieces[x][y]
                        if turn == "WHITE":
                            if piece.isupper():
                                select = True
                                chessboard[x][y] = 'Y'
                                move = Move(x,y)
                                if piece == "N":
                                    move.Knight()
                                if piece == "K":
                                    move.King()
                                if piece == "R":
                                    move.Rook()
                                if piece == "B":
                                    move.Bishop()
                                if piece == "P":
                                    move.whitePawn()
                                if piece == "Q":
                                    move.Queen()
                                
                        else:
                            if piece.islower():
                                select = True
                                chessboard[x][y] = 'Y'
                                move = Move(x,y)
                                if piece == "n":
                                    move.Knight()
                                if piece == "k":
                                    move.King()
                                if piece == "r":
                                    move.Rook()
                                if piece == "b":
                                    move.Bishop()
                                if piece == "p":
                                    move.blackPawn()
                                if piece == "q":
                                    move.Queen()
            else:
                newx, newy = get_mouse_position()
                newpos = [newx, newy]

                if piece.upper() == "N":
                    if newpos in move.Knight():
                        moving(x, y, newx, newy)
                        turn = switch_turn(turn)
                        pygame.mixer.Sound.play(makemove)
                    select = False

                # Contains castling
                if piece.upper() == "K":
                    if newpos in move.King():
                        moving(x, y, newx, newy)
                        if x == 4 and newx == 6:
                            moving(7,y,5,y)
                        elif x == 4 and newx == 2:
                            moving(0,y,3,y)
                        turn = switch_turn(turn)
                        pygame.mixer.Sound.play(makemove)
                    select = False

                if piece.upper() == "B":
                    if newpos in move.Bishop():
                        moving(x, y, newx, newy)
                        turn = switch_turn(turn)
                        pygame.mixer.Sound.play(makemove)
                    select = False
                
                if piece.upper() == "Q":
                    if newpos in move.Queen():
                        moving(x, y, newx, newy)
                        turn = switch_turn(turn)
                        pygame.mixer.Sound.play(makemove)
                    select = False
                
                if piece.upper() == "R":
                    if newpos in move.Rook():
                        moving(x, y, newx, newy)
                        turn = switch_turn(turn)
                        pygame.mixer.Sound.play(makemove)
                    select = False
                
                # white pawn
                if piece == "P":
                    if newpos in move.whitePawn():
                        moving(x, y, newx, newy)
                        # Pawn auto-promotion to Queen
                        if newy == 0:
                            chess_pieces[newx][newy] = 'Q'
                        turn = switch_turn(turn)
                        pygame.mixer.Sound.play(makemove)
                    select = False
                
                # black pawn
                if piece == "p":
                    if newpos in move.blackPawn():
                        moving(x, y, newx, newy)
                        # Promotion
                        if newy == 7:
                            chess_pieces[newx][newy] = 'q'
                        turn = switch_turn(turn)
                        pygame.mixer.Sound.play(makemove)
                    select = False

    # Resetting the board so that no squares are highlighted, when no item is selected
    if select == False:
        chessboard = []
        board = open("chessboard.txt", "r")
        for h in board.readlines():
            row = list(h.strip())
            chessboard.append(row)

    # If a king has been taken off the board, that means the game has been won or lost
    whiteKingPresent = True
    blackKingPresent = True
    if not any('K' in b for b in chess_pieces):
        whiteKingPresent = False
    if not any('k' in b for b in chess_pieces):
        blackKingPresent = False
    if whiteKingPresent == False:
        whitewin = False
        break
    if blackKingPresent == False:
        whitewin = True
        break

    # If you run out of time, you lose
    if display_time() == 'White':
        whitewin = True
        break
    elif display_time() == 'Black':
        whitewin = False
        break

    # Displaying everything, using subroutines described earlier
    display_board()
    display_pieces()
    display_turn()
    display_time()
    pygame.display.update()

# Displays game over message in a seperate loop
while True:
    clock.tick(40)
    events = pygame.event.get()
    for event in events:
        if event.type == pygame.QUIT:
            pygame.quit()
            break
        if event.type == pygame.KEYDOWN and event.key == pygame.K_ESCAPE:
            pygame.quit()
            break
    screen.fill(BLACK)
    pygame.font.init()
    myfont = pygame.font.SysFont("Comic Sans MS", 40)
    if whitewin:
        text = myfont.render("White wins!", False, WHITE)
    else:
        text = myfont.render("Black wins!", False, WHITE)
    screen.blit(text, (255, 290))
    pygame.display.update()