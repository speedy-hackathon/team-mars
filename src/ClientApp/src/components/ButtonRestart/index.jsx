import React from "react";
import {gameRestartUrl} from "../../consts/urls";
import errorHandler from "../../utils/errorHandler";
import styles from "./style.module.css";

export default class ButtonRestart extends React.Component {
  restart() {
    fetch(gameRestartUrl, {
      method: "POST"
    }).then(errorHandler);
  }

  render() {
    return (
      <div className={styles.container}>
        <button
          className={styles.button}
          type="button"
          onClick={() => {this.restart()}}
        >
          Рестарт
        </button>
      </div>
    )
  }
}