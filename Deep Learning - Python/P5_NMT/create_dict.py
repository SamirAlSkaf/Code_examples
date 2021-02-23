import pprint
from os import listdir
from os.path import isfile, join

file_directory = './text2text'
chars_to_remove = ['.', ',', '?', '!']

all_files = [join(file_directory, f) for f in listdir(file_directory) if isfile(join(file_directory, f)) and (f.startswith('iso') or f.startswith('con'))]

word_id_eng, word_id_deu = 0, 0

def process_files(filelist):
    global word_id_eng, word_id_deu

    dict_eng = {
        0: '<s>',
        1: '</s>'
    }
    dict_eng_inv = {
        '<s>': 0,
        '</s>': 1
    }
    dict_deu = {
        0: '<s>',
        1: '</s>'
    }
    dict_deu_inv = {
        '<s>': 0,
        '</s>': 1
    }

    word_id_eng, word_id_deu = 2, 2
    
    for filename in filelist:
        with open(filename) as f:
            # extract relevant contents
            content = f.readlines()
            content_eng = content[-2].replace('annot_eng', '').replace('transl_eng', '').lower().strip()
            content_deu = content[-1].replace('annot_deu', '').replace('transl_deu', '').lower().strip()

            # split all words and unify abostrophes
            content_eng = content_eng.replace('|', ' ').replace('`', '\'').replace('´', '\'')
            content_deu = content_deu.replace('|', ' ').replace('`', '\'').replace('´', '\'')

            # remove punctuation marks
            for char in chars_to_remove:
                content_eng = content_eng.replace(char, '')
                content_deu = content_eng.replace(char, '')

            # add new words to eng dict
            for word in content_eng.split(' '):
                if word not in dict_eng_inv:
                    dict_eng[word_id_eng] = word
                    dict_eng_inv[word] = word_id_eng
                    word_id_eng += 1
            
            # add new words to deu dict
            for word in content_deu.split(' '):
                if word not in dict_deu_inv:
                    dict_deu[word_id_deu] = word
                    dict_deu_inv[word] = word_id_deu
                    word_id_deu += 1

    return dict_eng, dict_eng_inv, dict_deu, dict_deu_inv

dict_eng, dict_eng_inv, dict_deu, dict_deu_inv = process_files(all_files)

#pp = pprint.PrettyPrinter(indent=4)
#pp.pprint(dict_eng)
#print('unique words eng:', word_id_eng)
#print('unique words deu:', word_id_deu)