import React from "react";
import styles from "./style.module.css";
import Field from "../Field";
import { DELAY, MAX_HEIGHT, MAX_WIDTH } from "../../consts/sizes";
import { gameStateUrl, userActionUrl } from "../../consts/urls";
import errorHandler from "../../utils/errorHandler";
import Instruction from "../Instruction";
import ButtonRestart from "../ButtonRestart";
import ButtonDayNight from "../ButtonDayNight";
import Timer from "../Timer";

import "./base.css";

export default class App extends React.Component {
  constructor() {
    super();
    this.state = {
      people: [],
      map: [],
      instructionOpen: true,
      isNight: true,
      localTicks: 0,
      ticks: 0,
    };
    this.intervalId = null;
    this.timerNight = null;
  }

  componentWillUnmount() {
    if (this.intervalId) {
      clearInterval(this.intervalId);
    }

    if (this.timerNight) {
      clearInterval(this.timerNight)
    }
  }

  checkNight(isNight) {
    return isNight ? 'night' : '';
  }

  render() {
    const { people, map, instructionOpen, ticks, isNight } = this.state;

    return (
      <div className={`${styles.root} app-container ${this.checkNight(isNight)}`}>
        {instructionOpen && <Instruction onClose={this.closeInstruction} />}
        <h1 className={styles.title}>Симулятор COVID</h1>
        <ButtonRestart />
        <ButtonDayNight
          changeNight={this.changeNight}
          isNight={isNight}
        />
        <Timer ticks={ticks} />
        <Field map={map} people={people} onClick={this.personClick} />
      </div>
    );
  }

  closeInstruction = () => {
    this.setState({
      instructionOpen: false,
    });

    this.getNewStateFromServer();
    this.setLocalTicks(this.state.localTicks, this.state.isNight);

    this.intervalId = setInterval(this.getNewStateFromServer, DELAY);
    this.timerNight = setInterval(() => {
      this.setLocalTicks(this.state.localTicks, this.state.isNight)}, DELAY
    );
  };

  setLocalTicks = (localTicks, isNight) => {
    if (localTicks % 10 === 0) {
      this.setState({
        isNight: !isNight
      })
    }

    this.setState({
      localTicks: ++localTicks,
    });
  }

  personClick = (id) => {
    fetch(userActionUrl, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        personClicked: id,
      }),
    }).then(errorHandler);
  };

  getNewStateFromServer = () => {
    fetch(gameStateUrl)
      .then(errorHandler)
      .then((res) => res.json())
      .then((game) => {
        this.setState({
          people: game.people,
          map: game.map.houses.map((i) => i.coordinates.leftTopCorner),
          ticks: game.ticks
        });
      });
  };

  changeNight = (isNight) => {
    this.setState({
      isNight: !isNight,
      localTicks: 0,
    })
  };
}
