from utils import urandom, isValidPrivKey, decode_privkey, encode_privkey, encode_pubkey, privkey_to_pubkey


def generate(name):
    valid = False
    while not valid:
        private_key = urandom(32)
        valid = isValidPrivKey(decode_privkey(private_key))

    with open('{}.priv'.format(name), 'wb') as f:
        f.write(private_key)

    with open('{}.pub'.format(name), 'wb') as f:
        f.write(privkey_to_pubkey(private_key))


if __name__ == "__main__":
    generate('cipher')
    generate('bitcoin')
