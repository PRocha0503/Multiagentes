from mesa import Agent

class RowAgent(Agent):
    """
    Row agent
    input: unique id, model
    output: row agent
    """

    def __init__(self,id,model,direction) -> None:
        """
        Inicialize the row agent
        input: id,model
        """
        super().__init__(id,model)
        self.direction = direction
        
    def step(self):
        """
        Step function
        """
        pass
