FROM python:3.14-slim

RUN apt-get update \
  && apt-get install -y git \
  && useradd -m apiuser \
  && mkdir /home/apiuser/ablapi \
  && chown apiuser /home/apiuser/ablapi

COPY --chown=apiuser --chmod=755 requirements.txt /home/apiuser/requirements.txt

# install pip packages
RUN pip install --upgrade -r /home/apiuser/requirements.txt \
  && pip install gunicorn

COPY --chown=apiuser --chmod=755 src/ablapi/ /home/apiuser/ablapi/

WORKDIR /home/apiuser

USER apiuser

ENV log_level="debug"

ENTRYPOINT [ "gunicorn", "-b", "0.0.0.0:8000", "--workers", "1", "--threads", "4", "--worker-class", "gthread", "ablapi:run()" ]
