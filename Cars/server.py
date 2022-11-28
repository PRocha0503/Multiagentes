from flask import Flask, request, jsonify
from model import  CarsModel

app = Flask("agentes")

carsModel = 0
currentStep = 0

def getDimensionsFromFile(filename):
    """
    Get dimensions of the grid from the file
    input: filename
    output: x,y
    """
    with open(filename) as f:
        data = f.read()
    data = data.split("\n")
    x = len(data[0])
    y = len(data)
    return x,y




@app.route('/init', methods=['POST'])
def init():
    global carsModel,currentStep
    currentStep = 0
    filename = request.json['filename']
    maxFrames = request.json['maxFrames']
    width,height = getDimensionsFromFile(filename)
    carsModel = CarsModel(filename,width,height)
    return jsonify({"message":"Parameters recieved, model initiated."})


@app.route('/cars', methods=['GET'])
def getRobots():
    global carsModel
    return jsonify({"cars":carsModel.getCars()})

@app.route('/trafficLights', methods=['GET'])
def getTrafficLights():
    global carsModel
    return jsonify({"positions":carsModel.getTrafficLights()})

@app.route('/step', methods=['GET'])
def step():
    global carsModel, currentStep
    carsModel.step()
    currentStep += 1
    return jsonify({'message':f'Model updated to step {currentStep}.', 'currentStep':currentStep})

if __name__=='__main__':
    app.run(host="localhost", port=8585, debug=True)